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

        private static string SERVER_ADDR = "127.0.0.1";
        private static int SERVER_PORT = 55555;
        private static string USERNAME = "MHMR";

        private static int ID = -1;
        private static int partner_id;
        private static int enemy1;
        private static int enemy2;

        private static string strong;

        private static Card[] pack;
        private static List<Card> playedStrongCards = new List<Card>();
        private static int[] points = new int[2];
        private static int counter = -1;
        private static int index = -1;

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

            Thread.Sleep(500);
            // Close the title screen:
            form.Controls.Find("titleScreen1", false)[0].Dispose();
            FirstFiveScreen firstFiveScreen = (FirstFiveScreen)form.Controls.Find("firstFiveScreen1", false)[0];
            firstFiveScreen.Controls.Find("ruler", false)[0].Text = "Ruler: " + ruler_id;
            if (ruler_id == ID)
                firstFiveScreen.Controls.Find("ruler", false)[0].Text += "(You)";
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

            Thread.Sleep(2000);
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

            form.SuspendLayout();
            gameScreen.Dispose();
            EndScreen end = (EndScreen)form.Controls.Find("endScreen1", false)[0];
            if (points[0] == 7)
            {
                end.Controls.Find("label1", false)[0].Text = "You won!";
                end.Controls.Find("label2", false)[0].Text = "Your points: 7\nEnemy points: " + points[1] + "\nCongratulations!";
            }
            else
            {
                end.BackColor = Color.FromArgb(246, 76, 57);
                end.Controls.Find("label1", false)[0].Text = "You lost...";
                end.Controls.Find("label2", false)[0].Text = "Your points: " + points[0] + "\nEnemy points: 7\nBetter luck next time...";
            }
            form.ResumeLayout();

            Networking.CloseSocket(sock);
        }

        /// <summary>
        /// Gets the pack data from the server and creates the 'pack' array.
        /// </summary>
        /// <param name="mes"> The message from the server. </param>
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
                else
                {
                    if (ID == 1 || ID == 4)
                    {
                        enemy1 = int.Parse(team.Split('+')[1]);  // bigger
                        enemy2 = int.Parse(team.Split('+')[0]);  // smaller
                    }
                    else
                    {
                        enemy1 = int.Parse(team.Split('+')[0]);  // smaller
                        enemy2 = int.Parse(team.Split('+')[1]);  // bigger
                    }
                }
            }
        }

        /// <summary>
        /// Simulates the game.
        /// </summary>
        /// <param name="sock"> The socket to the server. </param>
        /// <param name="screen"> The screen in which the game is displayed (to show the turns playing in the GUI). </param>
        public static void Play(Socket sock, GameScreen screen)
        {
            bool isGame = true;
            Card result;
            string format;

            while (isGame)
            {
                string mes = Networking.RecvMessage(sock);
                result = DoTurn(mes);
                if (result == null)
                    return;
                format = "play_card:" + result.GetCardType() + "*" + result.GetCardRank();
                Console.WriteLine(format);
                Networking.SendMessage(sock, format);
                string response = Networking.RecvMessage(sock);
                if (response == "bad_play")
                    return;

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

                screen.ShowTurn((4 - counter) % 4, index, formatCard(playedCards[(partner_id - 1) % 4]),
                    formatCard(playedCards[(enemy1 - 1) % 4]), formatCard(playedCards[(enemy2 - 1) % 4]));
                waitHandle.WaitOne();
                ScreenBlink(screen, isWinner);
                screen.UpdatePoints(points[0], points[1]);

                REAL_STARTEGY.Discover(partner_id, strong, counter, playedCards, playedStrongCards);
            }
        }


        /// <summary>
        /// this function is called each turn, it receives the message from the server as input, 
        /// analyses it, extracts the needed data and variables, calls the strategy to choose our card, and returns it. e
        /// </summary>
        /// <param name="mes"> The message from the server. </param>
        /// <returns></returns>
        public static Card DoTurn(string mes)
        {
            if (mes == "GAME_OVER")
                return null;

            string[] data = mes.Split(',');
            string suit = data[0].Split(':')[1];
            string[] cards_str = data[1].Split(':')[1].Split('|');
            Console.WriteLine(data[1].Split(':')[1]);
            Card[] played_cards = new Card[cards_str.Length];
            counter = 0;
            for (int i = 0; i < cards_str.Length; i++)
                if (cards_str[i] != "")
                {
                    played_cards[i] = new Card(cards_str[i].Split('*')[0], cards_str[i].Split('*')[1]);
                    counter++;
                }

            Card selected = REAL_STARTEGY.DoTurn(ID, partner_id, suit, strong, points[0], points[1], played_cards, counter, pack, playedStrongCards);
            index = Array.IndexOf(pack, selected);
            pack[index] = null;

            return selected;
        }

        /// <summary>
        /// Displays a "you win" or "you lose" message.
        /// </summary>
        /// <param name="root"> The game screen. </param>
        /// <param name="isWinner"> Did we win or not. </param>
        public static void ScreenBlink(GameScreen root, bool isWinner)
        {
            if (isWinner)
                root.Controls.Find("winText", false)[0].Text = "You win!";
            else
                root.Controls.Find("winText", false)[0].Text = "You lose...";
            Thread.Sleep(500);
            root.Controls.Find("winText", false)[0].Text = "";
        }
    }
}
