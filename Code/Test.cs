using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace HOKM.Code
{
    internal class Test
    {

        private static string SERVER_ADDR = "10.0.0.4";
        private static int SERVER_PORT = 55555;
        private static int ID = -1;
        private static string USERNAME = "Name";

        private static Card[] pack;
        private static int partner_id;
        private static string strong;

        private static List<Card> card_history = new List<Card>();
        private static int[] points = new int[2];

        private static int enemy1=0;
        private static int enemy2=0;
        private static string[] discover = new string[5];

        //0-SPADES, 1-CLUBS, 2-DIAMONDS, 3-HEARTS
        private static int[] big_card = {14, 14, 14, 14};

        public static void Main(string[] args)
        {
            Socket sock = Networking.OpenSocket(SERVER_ADDR, SERVER_PORT);

            // Receive Client ID:
            string id_text = Networking.RecvMessage(sock);
            ID = int.Parse(id_text.Split(':')[1]);
            Console.WriteLine("This client's ID is " + ID);

            // Send Username:
            // Networking.SendMessage(sock, "username:" + USERNAME);

            // Determine Strong:
            strong = GetStrong(sock);
            if (strong != null)
                while (true)
                {
                    Networking.SendMessage(sock, "set_strong:" + strong);
                    if (Networking.RecvMessage(sock) == "ok")
                        break;
                }

            BuildPack(sock);

            bool isGame = true;
            string result;
            int turn;

            // Play:
            while (isGame)
            {
                result = DoTurn(sock);
                turn = int.Parse(result.Split('p')[0]);
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
                            result = DoTurn(sock); //do something to change the selected card
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

                for (int i = 0; i < 4; i++)
                {
                    string card_data = datarr[2].Split(':')[1].Split('|')[i];
                    Card c = new Card(card_data.Split('*')[0], card_data.Split('*')[1]);
                    c.SetPlayer(i + 1);
                    int cardTurn = (ID + 4 - turn) % 4;
                    if (cardTurn == 2) cardTurn = 3;
                    else if (cardTurn == 3) cardTurn = 2;
                    c.SetTurn(cardTurn);
                    card_history.Add(c);
                }
            }

            Networking.CloseSocket(sock);
        }

        public static string GetStrong(Socket sock)
        {
            string mes = Networking.RecvMessage(sock);
            int ruler_id = int.Parse(mes.Split(':')[1]);
            mes = Networking.RecvMessage(sock);

            if (ruler_id == ID)
            {
                Console.WriteLine("I am the ruler");
                string five_mes = mes;
                string[] five_string_arr = five_mes.Split('|');
                Card[] first_five = new Card[5];
                for (int i = 0; i < 5; i++)
                    first_five[i] = new Card(five_string_arr[i].Split('*')[0], five_string_arr[i].Split('*')[1]);

                string strong = "HEARTS";  // Placeholder
                // Algorithm
                return strong;
            }

            return null;
        }

        public static void BuildPack(Socket sock)
        {
            string mes = Networking.RecvMessage(sock);

            string[] data = mes.Split(',');
            string[] data_cards  = data[0].Split('|');
            string[] teams = data[1].Split(':')[1].Split('|');
            strong = data[2].Split(':')[1];

            pack = new Card[13];
            for (int i=0; i < data_cards.Length; i++)
                pack[i]  =  new Card(data_cards[i].Split('*')[0], data_cards[i].Split('*')[1]);

            foreach (string team in teams) {
                if (team.Contains(ID.ToString()))
                {
                    if (int.Parse(team.Split('+')[0]) == ID)
                        partner_id = int.Parse(team.Split('+')[1]);
                    else
                        partner_id = int.Parse(team.Split('+')[0]);
                }
            }
        }

        public static string DoTurn(Socket sock)
        {
            string mes = Networking.RecvMessage(sock);

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

            Card selected = pack[0];
            // Algorithm
            string format = counter + "played_card:" + selected.GetCardType() + "*" + selected.GetCardRank();
            return format;
        }
        public static int GetBegginer(int counter)
        {
            switch (counter)
            {
                //1 3 2 4
                case 0:
                    //im first
                    return ID;
                case 1:
                    //im second
                    if (ID == 1)
                        return 4;
                    if (ID == 2)
                        return 3;
                    if (ID == 3)
                        return 1;
                    if (ID == 4)
                        return 2;
                case 2:
                    //im third
                    if (ID == 1)
                        return 2;
                    if (ID == 2)
                        return 1;
                    if (ID == 3)
                        return 4;
                    if (ID == 4)
                        return 3;
                case 3:
                    //im fourth
                    if (ID == 1)
                        return 3;
                    if (ID == 2)
                        return 4;
                    if (ID == 3)
                        return 2;
                    if (ID == 4)
                        return 1;

            }

        }

        public static void Discover(int counter, Card[] played_cards)
        {
            //updates the current biggest card in each suit
            foreach(Card card in played_cards)
            {
                if (card.GetCardType()=="SPADES" && card.GetValue() == big_card[0])
                    big_card[0]--;
                else if (card.GetCardType()=="CLUBS" && card.GetValue() == big_card[1])
                    big_card[1]--;
                else if (card.GetCardType()=="DIAMONDS" && card.GetValue() == big_card[2])
                    big_card[2]--;
                else if (card.GetCardType()=="HEARTS" && card.GetValue() == big_card[3])
                    big_card[3]--;
            }
            //1 3 2 4
    }

}
