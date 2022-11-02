using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using HOKM.Code;
using System.Net.Sockets;
using HOKM.Screens;
using System.Drawing;
using System.Threading;

namespace HOKM
{
    static class Program
    {

        private static string SERVER_ADDR = "10.0.0.4";
        private static int SERVER_PORT = 55555;
        private static string USERNAME = "Name";

        private static int ID = -1;
        private static int partner_id;
        private static string strong;

        private static Card[] pack;
        private static List<Card> playedStrongCards = new List<Card>();
        private static int[] points = new int[2];

        public static EventWaitHandle waitHandle = new AutoResetEvent(false);


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 form = new Form1();
            Thread gameThread = new Thread(() => GameThread(form));
            gameThread.Start();
            Application.Run(form);
        }

        static void GameThread(Form1 form)
        {
            Socket sock;

            try
            {
                sock = Networking.OpenSocket(SERVER_ADDR, SERVER_PORT);
            }
            catch
            {
                TitleScreen a = (TitleScreen)form.Controls.Find("titleScreen1", false)[0];
                a.Controls.Find("label3", false)[0].Text = "Could not connect to server.";
                return;
            }

            form.SuspendLayout();

            // Receive Client ID:
            string id_text = Networking.RecvMessage(sock);
            ID = int.Parse(id_text.Split(':')[1]);
            Console.WriteLine("This client's ID is " + ID);

            // Send Username:
            Networking.SendMessage(sock, "username:" + USERNAME);

            // Retrieving the ruler's ID.
            string mes = Networking.RecvMessage(sock);
            int ruler_id = int.Parse(mes.Split(':')[1]);

            // Getting the first five cards.
            mes = Networking.RecvMessage(sock);
            string[] five_string_arr = mes.Split('|');
            Card[] first_five = new Card[5];
            for (int i = 0; i < 5; i++)
                first_five[i] = new Card(five_string_arr[i].Split('*')[0], five_string_arr[i].Split('*')[1]);

            // Close the title screen:
            form.Controls.Find("titleScreen1", false)[0].Dispose();
            FirstFiveScreen firstFiveScreen = (FirstFiveScreen)form.Controls.Find("firstFiveScreen1", false)[0];
            for (int i = 0; i < 5; i++)
                firstFiveScreen.Controls.Find("card" + i, false)[0].BackgroundImage =
                    (Bitmap)Properties.Resources.ResourceManager.GetObject(
                        first_five[i].GetCardRank().Substring(5) + first_five[i].GetCardType().Substring(0, 1));
            form.ResumeLayout();

            if (ruler_id == ID)
            {
                // Determine Strong:
                strong = REAL_STARTEGY.GetStrong(first_five);
                while (true)
                {
                    Networking.SendMessage(sock, "set_strong:" + strong);
                    if (Networking.RecvMessage(sock) == "ok")
                        break;
                }
            }

            // Getting the pack from the server:
            BuildPack(Networking.RecvMessage(sock));

            Thread.Sleep(1500);
            form.SuspendLayout();
            // Close the title screen:
            form.Controls.Find("firstFiveScreen1", false)[0].Dispose();
            GameScreen gameScreen = (GameScreen)form.Controls.Find("gameScreen1", false)[0];
            for (int i = 0; i < 13; i++)
            {
                string name;
                if (i < 10)
                    name = "card0" + i;
                else
                    name = "card" + i;
                gameScreen.Controls.Find(name, false)[0].BackgroundImage =
                    (Bitmap)Properties.Resources.ResourceManager.GetObject(
                        pack[i].GetCardRank().Substring(5) + pack[i].GetCardType().Substring(0, 1));
            }
            form.ResumeLayout();

            gameScreen.UpdateStrong(strong[0]);

            // Play:
            Play(sock, gameScreen);

            Networking.CloseSocket(sock);
        }

        public static void BuildPack(string mes)
        {

            string[] data = mes.Split(',');
            string[] data_cards = data[0].Split('|');
            string[] teams = data[1].Split(':')[1].Split('|');
            strong = data[2].Split(':')[1];

            pack = new Card[13];
            int[] valueArray = new int[13];

            for (int i = 0; i < data_cards.Length; i++)
            {
                pack[i] = new Card(data_cards[i].Split('*')[0], data_cards[i].Split('*')[1]);
                valueArray[i] = pack[i].GetValue();
            }

            Array.Sort(valueArray, pack);

            foreach (string team in teams)
            {
                if (team.Contains(ID.ToString()))
                {
                    if (int.Parse(team.Split('+')[0]) == ID)
                        partner_id = int.Parse(team.Split('+')[1]);
                    else
                        partner_id = int.Parse(team.Split('+')[0]);
                }
            }
        }

        public static void Play(Socket sock, GameScreen screen)
        {
            bool isGame = true;
            string result;
            int turn;
            int index;

            while (isGame)
            {
                string mes = Networking.RecvMessage(sock);
                result = DoTurn(mes);
                turn = int.Parse(result.Substring(0,1));
                index = int.Parse(result.Substring(1,2));
                result = result.Substring(1);

                if (result == "GAME_OVER")
                    isGame = false;
                else
                    while (true)
                    {
                        Networking.SendMessage(sock, result);
                        
                        string response = Networking.RecvMessage(sock);
                        if (response == "ok")
                            break;
                        else if (response == "bad_play")
                            result = DoTurn(mes);
                    }

                // Round over:
                string data = Networking.RecvMessage(sock);

                string[] datarr = data.Split(',');

                bool isWinner = datarr[0].Split(':')[1] == (ID + "+" + partner_id) || datarr[0].Split(':')[1] == (partner_id + "+" + ID);

                foreach (string score_data in datarr[1].Split(':')[1].Split('|'))
                {
                    if (score_data.Split('*')[0] == (ID + "+" + partner_id) || score_data.Split('*')[0] == (partner_id + "+" + ID))
                        points[0] = int.Parse(score_data.Split('*')[1]);
                    else
                        points[1] = int.Parse(score_data.Split('*')[1]);
                }

                Card[] playedCards = new Card[4];

                for (int i = 0; i < 4; i++)
                {
                    string card_data = datarr[2].Split(':')[1].Split('|')[i];
                    Card c = new Card(card_data.Split('*')[0], card_data.Split('*')[1]);
                    playedCards[i] = c;
                }

                Func<Card, string> formatCard = (Card card) => card.GetCardRank().Substring(5) + card.GetCardType().Substring(0, 1);

                screen.ShowTurn((-turn) % 4, index, formatCard(playedCards[(turn + 2) % 4]),
                    formatCard(playedCards[(turn - 1) % 4]), formatCard(playedCards[(turn + 1) % 4]));
                waitHandle.WaitOne();
                ScreenBlink(screen, isWinner);
                screen.UpdatePoints(points[0], points[1]);

                REAL_STARTEGY.Discover(partner_id, strong, turn, playedCards, playedStrongCards);
            }
        }


        public static string DoTurn(string mes)
        {
            if (mes == "GAME_OVER")
                return "GAME_OVER";

            string[] data = mes.Split(',');
            string suit = data[0].Split(':')[1];
            string[] cards_str = data[1].Split(':')[1].Split('|');
            Card[] played_cards = new Card[cards_str.Length];
            int counter = 0;
            for (int i = 0; i < cards_str.Length; i++)
                if (cards_str[i] != "")
                {
                    played_cards[i] = new Card(cards_str[i].Split('*')[0], cards_str[i].Split('*')[1]);
                    counter++;
                }

            Card selected = REAL_STARTEGY.DoTurn(ID, partner_id, suit, strong, points[0], points[1], played_cards, counter, pack, playedStrongCards);

            int index = Array.IndexOf(pack, selected);
            string strIndex;
            if (index < 10)
                strIndex = "0" + index;
            else
                strIndex = index.ToString();

            string format = counter + strIndex + "played_card:" + selected.GetCardType() + "*" + selected.GetCardRank();
            return format;
        }

        public static void ScreenBlink(GameScreen root, bool isWinner)
        {
            if (isWinner)
                root.BackColor = Color.LightGreen;
            else
                root.BackColor = Color.OrangeRed;
            Thread.Sleep(100);
            root.BackColor = Color.Empty;
        }
    }
}
