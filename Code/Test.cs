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

        static char[] TYPES = { 'e', '2', '3', '4', '5', '6', '7', '8', '9', '0', 'J', 'Q', 'K', 'A', 'f' };

        static int GetPower(string type)
        {
            char pow = type[type.Length - 1];
            for (int i = 0; i < TYPES.Length; i++)
                if (TYPES[i] == pow)
                    return i;
            return -1;
        }

        private static string SERVER_ADDR = "10.0.0.4";
        private static int SERVER_PORT = 55555;
        private static int ID = -1;
        private static string USERNAME = "Name";

        private static Card[] pack;
        private static int partner_id;
        private static string strong;

        private static List<Card> card_history = new List<Card>();
        private static int[] points = new int[2];

        public static void Mmain(string[] args)
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
                string[] five_string_arr = mes.Split('|');
                Card[] first_five = new Card[5];
                for (int i = 0; i < 5; i++)
                    first_five[i] = new Card(five_string_arr[i].Split('*')[0], five_string_arr[i].Split('*')[1]);

                string strong = "HEARTS";  // Placeholder
                //0-SPADES, 1-CLUBS, 2-DIAMONDS, 3-HEARTS
                int[] types_count = {0, 0, 0, 0};
                int[] sum_cards = new int[4];

                for (int i=0; i < first_five.Length; i++)
                {
                    switch (first_five[i].GetCardType())
                    {
                        case "SPADES":
                            types_count[0]++;
                            sum_cards[0] += GetPower(first_five[i].GetCardRank());
                            break;
                        case "CLUBS":
                            types_count[1]++;
                            sum_cards[1] += GetPower(first_five[i].GetCardRank());
                            break;
                        case "DIAMONDS":
                            types_count[2]++;
                            sum_cards[2] += GetPower(first_five[i].GetCardRank());
                            break;
                        case "HEARTS":
                            types_count[3]++;
                            sum_cards[3] += GetPower(first_five[i].GetCardRank());
                            break;
                    }
                }

                int max = 0;
                int index = 0;
                for (int i = 0; i < 4; i++)
                {
                    if (types_count[i] * sum_cards[i] > max)
                    {
                        index = i;
                        max = types_count[i] * sum_cards[i];
                    }
                }

                string[] names = { "SPADES", "CLUBS", "DIAMONDS", "HEARTS" };

                return names[index];


                //int max_cards = types_count[0];
                //for (int i=0; i < types_count.Length; i++)
                //{
                //    if (types_count[i] > max_cards)
                //    {
                //        max_cards = types_count[i];
                //        switch (i)
                //        {
                //            case 0:
                //                strong="SPADES";
                //                break;
                //            case 1:
                //                strong="CLUBS";
                //                break;
                //            case 2:
                //                strong = "DIAMONS";
                //                break;
                //            case 3:
                //                strong = "HEARTS";
                //                break;
                //        }
                //    }
                //}
                //for (int i=0; i<types_count.Length; i++)
                //{
                //    if (max_cards == types_count[i])
                //    {
                //        int[] max_count_values = new int[2];
                //        int count_max = 0;

                //        int[] new_max = new int[2];
                //        int new_count=0;
                //        string new_strong="";
                //        Card card=new Card("DIAMONDS", "rank_2");
                //        switch (types_count[i])
                //        {
                //            case 0:
                //                new_strong="SPADES";
                //                break;
                //            case 1:
                //                new_strong="CLUBS";
                //                break;
                //            case 2:
                //                new_strong = "DIAMONS";
                //                break;
                //            case 3:
                //                new_strong = "HEARTS";
                //                break;
                //        }

                //        for (int j=0; j < first_five.Length; j++)
                //        {
                //            if (first_five[i].GetCardType() == strong)
                //            {
                //                max_count_values[count_max] = first_five[i].GetValue();
                //                count_max++;
                //            }
                //            else if (first_five[i].GetCardType() == new_strong)
                //            {
                //                new_max[new_count]=first_five[i].GetValue();
                //                new_count++;
                //            }
                //            else
                //                card=first_five[i];
                //        }
                //        int maxNumFirst = Math.Max(max_count_values[0], max_count_values[1]);
                //        int minNumFirst = Math.Min(max_count_values[0], max_count_values[1]);

                //        int maxNumSec = Math.Max(new_max[0], new_max[1]);
                //        int minNumSec = Math.Min(new_max[0], new_max[1]);
                //        if (maxNumFirst>maxNumSec && minNumFirst>minNumSec)
                //            if (maxNumFirst>10 && minNumFirst>5)
                //                return strong;
                //            else
                //            {
                //                if (card.GetValue()>12)
                //                    return card.GetCardType();
                //                else
                //                    return strong;
                //            }
                                
                                    
                //        else if (maxNumFirst<maxNumSec && minNumFirst<minNumSec)
                //                if (maxNumSec>10 && minNumSec>5)
                //                    return new_strong;
                //                else
                //                {
                //                    if (card.GetValue()>12)
                //                            return card.GetCardType();
                //                    else
                //                        return new_strong;
                //                }
                //        else
                //        {
                //            if (maxNumFirst>maxNumSec)
                //                return strong;
                //            else
                //               return new_strong;
                //        }

                //    }
                //}

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
    }

}
