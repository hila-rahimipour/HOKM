using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace HOKM.Code
{
    internal class Strategy
    {

        static char[] TYPES = { 'e', '2', '3', '4', '5', '6', '7', '8', '9', '0', 'J', 'Q', 'K', 'A', 'f' };

        private static string SERVER_ADDR = "10.0.0.4";
        private static int SERVER_PORT = 55555;
        private static int ID = -1;
        private static string USERNAME = "Name";

        private static Card[] pack;
        private static int partner_id;
        private static string strong;

        private static List<Card> card_history = new List<Card>();
        private static int[] points = new int[2];

        private static int enemy1 = 0;
        private static int enemy2 = 0;
        private static string[] discover = new string[5];

        private static int count = -1;

        //0-SPADES, 1-CLUBS, 2-DIAMONDS, 3-HEARTS
        private static int[] big_card = { 14, 14, 14, 14 };

        static int GetPower(string type)
        {
            char pow = type[type.Length - 1];
            for (int i = 0; i < TYPES.Length; i++)
                if (TYPES[i] == pow)
                    return i;
            return -1;
        }

        public static Card GetCard(string suit, int turn, Card[] pack, Card[] played, List<Card> memory)
        {

            Card selected = new Card(suit, "f");
            string highest = "e";

            if (turn == 3)
            {
                // Finding the highest card played:
                foreach (Card card in played)
                    if (card.GetCardType() == suit && GetPower(card.GetCardRank()) > GetPower(highest))
                        highest = card.GetCardRank();

                if (highest == "e")
                    highest = "f";

                // Checking if I have a winning card of said suit:
                string min = "f";

                foreach (Card card in pack)
                    if (card.GetCardType() == suit &&
                        GetPower(card.GetCardRank()) == GetPower(highest) - 1 &&
                        GetPower(card.GetCardRank()) < GetPower(selected.GetCardRank()) &&
                        GetPower(card.GetCardRank()) < GetPower(min))
                    {
                        selected.SetRank(card.GetCardRank());
                    }

                // Playing my lowest card if not:
                if (selected.GetCardRank() == "f")
                    foreach (Card card in pack)
                        if (GetPower(card.GetCardRank()) < GetPower(selected.GetCardRank()))
                        {
                            selected.SetType(card.GetCardType());
                            selected.SetRank(card.GetCardRank());
                        }
            }

            else
            {
                // Finding the highest card played in this suit:
                foreach (Card card in memory)
                    if (card.GetCardType() == suit && GetPower(card.GetCardRank()) > GetPower(highest))
                        highest = card.GetCardRank();

                foreach (Card card in played)
                    if (card.GetCardType() == suit && GetPower(card.GetCardRank()) > GetPower(highest))
                        highest = card.GetCardRank();

                if (highest == "e")
                    highest = "f";

                // Checking if I have the highest card of said suit:
                foreach (Card card in pack)
                    if (card.GetCardType() == suit &&
                        GetPower(card.GetCardRank()) == GetPower(highest) - 1 &&
                        GetPower(card.GetCardRank()) < GetPower(selected.GetCardRank()))
                    {
                        selected.SetRank(card.GetCardRank());
                    }

                // Playing my lowest card if not:
                if (selected.GetCardRank() == "f")
                    foreach (Card card in pack)
                        if (GetPower(card.GetCardRank()) < GetPower(selected.GetCardRank()))
                        {
                            selected.SetType(card.GetCardType());
                            selected.SetRank(card.GetCardRank());
                        }
            }

            return selected;

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

                //0-SPADES, 1-CLUBS, 2-DIAMONDS, 3-HEARTS
                int[] types_count = { 0, 0, 0, 0 };
                for (int i = 0; i < first_five.Length; i++)
                {
                    switch (first_five[i].GetCardType())
                    {
                        case "SPADES":
                            types_count[0]++;
                            break;
                        case "CLUBS":
                            types_count[1]++;
                            break;
                        case "DIAMONDS":
                            types_count[2]++;
                            break;
                        case "HEARTS":
                            types_count[3]++;
                            break;
                    }
                }
                int max_cards = types_count[0];
                int strong_id = 0;
                for (int i = 1; i < types_count.Length; i++)
                {
                    if (types_count[i] > max_cards)
                    {
                        max_cards = types_count[i];
                        strong_id = i;
                        switch (strong_id)
                        {
                            case 0:
                                strong = "SPADES";
                                break;
                            case 1:
                                strong = "CLUBS";
                                break;
                            case 2:
                                strong = "DIAMONS";
                                break;
                            case 3:
                                strong = "HEARTS";
                                break;
                        }
                    }
                }
                for (int i = 0; i < types_count.Length; i++)
                {
                    if (max_cards == types_count[i])
                    {
                        int[] max_count_values = new int[2];
                        int count_max = 0;

                        int[] new_max = new int[2];
                        int new_count = 0;
                        string new_strong = "";
                        Card card = new Card("DIAMONDS", "rank_2");
                        switch (types_count[i])
                        {
                            case 0:
                                new_strong = "SPADES";
                                break;
                            case 1:
                                new_strong = "CLUBS";
                                break;
                            case 2:
                                new_strong = "DIAMONS";
                                break;
                            case 3:
                                new_strong = "HEARTS";
                                break;
                        }

                        for (int j = 0; j < first_five.Length; j++)
                        {
                            if (first_five[i].GetCardType() == strong)
                            {
                                max_count_values[count_max] = first_five[i].GetValue();
                                count_max++;
                            }
                            else if (first_five[i].GetCardType() == new_strong)
                            {
                                new_max[new_count] = first_five[i].GetValue();
                                new_count++;
                            }
                            else
                                card = first_five[i];
                        }
                        int maxNumFirst = Math.Max(max_count_values[0], max_count_values[1]);
                        int minNumFirst = Math.Min(max_count_values[0], max_count_values[1]);

                        int maxNumSec = Math.Max(new_max[0], new_max[1]);
                        int minNumSec = Math.Min(new_max[0], new_max[1]);
                        if (maxNumFirst > maxNumSec && minNumFirst > minNumSec)
                            if (maxNumFirst > 10 && minNumFirst > 5)
                                return strong;
                            else
                            {
                                if (card.GetValue() > 12)
                                    return card.GetCardType();
                                else
                                    return strong;
                            }


                        else if (maxNumFirst < maxNumSec && minNumFirst < minNumSec)
                            if (maxNumSec > 10 && minNumSec > 5)
                                return new_strong;
                            else
                            {
                                if (card.GetValue() > 12)
                                    return card.GetCardType();
                                else
                                    return new_strong;
                            }
                        else
                        {
                            if (maxNumFirst > maxNumSec)
                                return strong;
                            else
                                return new_strong;
                        }

                    }
                }
                ;
            }

            return null;
        }

        public static void BuildPack(Socket sock)
        {
            string mes = Networking.RecvMessage(sock);

            string[] data = mes.Split(',');
            string[] data_cards = data[0].Split('|');
            string[] teams = data[1].Split(':')[1].Split('|');
            strong = data[2].Split(':')[1];

            pack = new Card[13];
            for (int i = 0; i < data_cards.Length; i++)
                pack[i] = new Card(data_cards[i].Split('*')[0], data_cards[i].Split('*')[1]);

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

        public static string DoTurn(Socket sock)
        {
            string mes = Networking.RecvMessage(sock);

            if (mes == "GAME_OVER")
                return "GAME_OVER";
            else if (mes.Contains("round end"))
            {
                Card[] round_card = new Card[4];
                string[] cards = mes.Substring(mes.IndexOf("round_cards: ") + 13).Split('|');
                for (int i = 0; i < round_card.Length; i++)
                {
                    round_card[i] = new Card(cards[i].Split('*')[0], cards[i].Split('*')[1]);
                }
                Discover(count, round_card);


            }

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
        public static int[] GetOrder(int counter)
        {
            // Order 
            int[] order = new int[4] { 1, 3, 2, 4 };
            // Empty array to fill in, and return
            int[] return_value = new int[4]; //1, 3, 2, 4, 1, 3, 4, 2    ← Looping the Order array
                                             // Circle around the array ( E.x.        2, 4, 1, 3       )

            for (int i = 0; i < 4; i++)
            {
                // counter (1-3), move to (0-2) 
                return_value[i] = order[(counter - 1 + i) % 4];
            }
            // Return the array we just filled in
            return return_value;
        }

        public static void Discover(int counter, Card[] played_cards)
        {
            //updates the current biggest card in each suit
            foreach (Card card in played_cards)
            {
                if (card.GetCardType() == "SPADES" && card.GetValue() == big_card[0])
                    big_card[0]--;
                else if (card.GetCardType() == "CLUBS" && card.GetValue() == big_card[1])
                    big_card[1]--;
                else if (card.GetCardType() == "DIAMONDS" && card.GetValue() == big_card[2])
                    big_card[2]--;
                else if (card.GetCardType() == "HEARTS" && card.GetValue() == big_card[3])
                    big_card[3]--;
            }
            //1 3 2 4

            Card first_card = played_cards[GetOrder(counter)[0]];
            for (int i = 1; i < played_cards.Length; i++)
            {
                if (first_card.GetCardType() != played_cards[i].GetCardType() && first_card.GetCardType() != strong)
                    discover[i + 1] = discover[i + 1] + "KILL " + first_card.GetCardType() + "|";
            }
            if (first_card.GetCardType() == strong)
                for (int i = 0; i < played_cards.Length; i++)
                    if (first_card.GetCardType() != played_cards[i].GetCardType())
                        discover[i + 1] = discover[i + 1] + "NO STRONG|";
            int current_winner = GetCurrentWinner(played_cards, counter);
            Card winner_card = GetCurrentWinnerCard(played_cards, counter);
            int[] order = GetOrder(counter);
            int first_player = order[0];
            if (first_player != partner_id)
            {
                Card partner_card = played_cards[partner_id - 1];
                if (winner_card.GetCardType() == first_card.GetCardType())
                    if (partner_card.GetCardType() == winner_card.GetCardType())
                        if (winner_card.GetValue() - partner_card.GetValue() < 3)
                            discover[partner_id] = discover[partner_id] + "KILL " +
                                partner_card.GetCardType() + "|";

            }

        }
        public static Card killSmall(int counter, Card[] played_cards)
        {
            int begginer = GetOrder(counter)[0];
            Card first_card = played_cards[begginer];
            bool have_type = false;
            for (int i = 0; i < pack.Length; i++)
            {
                if (first_card.GetCardType() == pack[i].GetCardType())
                    have_type = true;
            }
            bool killed_enemy = false;
            Card killer = null;
            for (int i = 0; i < played_cards.Length; i++)
            {
                if (first_card.GetCardType() != played_cards[i].GetCardType()
                    && played_cards[i].GetCardType() == strong)
                {
                    if (killer == null)
                        killer = played_cards[i];
                    else
                    {
                        if (killer.GetValue() < played_cards[i].GetValue())
                            killer = played_cards[i];
                    }
                    if (i + 1 == enemy1 || i + 1 == enemy2)
                    {
                        killed_enemy = true;
                    }

                }
            }

            int strong_counter = 0;
            Card my_card = new Card("DIAMONDS", "rank_A");
            if (!have_type && GetCurrentWinner(played_cards, counter) != partner_id)
            {
                if (!killed_enemy)
                {
                    for (int i = 0; i < pack.Length; i++)
                    {
                        if (pack[i].GetCardType() == strong)
                        {
                            strong_counter++;
                            if (pack[i].GetValue() < my_card.GetValue())
                                my_card = pack[i];
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < pack.Length; i++)
                    {
                        if (pack[i].GetCardType() == strong)
                        {
                            strong_counter++;
                            if (pack[i].GetValue() > killer.GetValue())
                                my_card = pack[i];
                        }
                    }
                }
            }
            if (strong_counter > 4 && my_card.GetValue() < 10)
                return my_card;
            else
            {
                Card minCard = new Card("DIAMONDS", "rank_A");
                for (int i = 0; i < pack.Length; i++)
                {
                    if (pack[i].GetValue() < minCard.GetValue())
                        minCard = pack[i];
                }
                return minCard;
            }
        }
        //if im first
        public static Card IfPartnerKillsSomething()
        {
            string[] partner_discover = discover[partner_id].Split('|');
            string[] kills = new string[4];
            for (int i = 0; i < partner_discover.Length; i++)
                if (partner_discover[i].Contains("KILL"))
                    kills[i] = partner_discover[i].Substring(partner_discover[i].IndexOf("KILL ") + 5);

            Card my_card = new Card("DIAMONDS", "rank_A");
            for (int i = 0; i < pack.Length; i++)
            {
                for (int j = 0; j < kills.Length; j++)
                    if (kills[i] != null)
                        if (pack[i].GetCardType() == kills[j])
                            if (my_card.GetValue() > pack[i].GetValue())
                                my_card = pack[i];

            }
            if (my_card.GetCardRank() != "rank_A" && my_card.GetCardType() != "DIAMONDS")
                return my_card;
            else
                return new Card("NOTHING", "NOTHING");
            //use other algorithem or put random card

        }
        public static int GetCurrentWinner(Card[] played_cards, int counter)
        {
            Card current_winner_card = played_cards[0];
            int current_winner_id = 1;
            for (int i = 1; i < played_cards.Length; i++)
            {
                if (current_winner_card.GetValue() < played_cards[i].GetValue()
                    && current_winner_card.GetCardType() == played_cards[i].GetCardType())
                {
                    current_winner_card = played_cards[i];
                    current_winner_id = i + 1;
                }
                else
                {
                    if (played_cards[i].GetCardType() == strong)
                    {
                        current_winner_card = played_cards[i];
                        current_winner_id = i + 1;
                    }
                }

            }
            return current_winner_id;
        }
        public static Card GetCurrentWinnerCard(Card[] played_cards, int counter)
        {
            return played_cards[GetCurrentWinner(played_cards, counter) - 1];
        }

    }

    class StrongStrategy
    {
        static string[] types = { "SPADES", "CLUBS", "DIAMONDS", "HEARTS" };
        static string most_common = "";
        static char[] TYPES = { 'e', '2', '3', '4', '5', '6', '7', '8', '9', '0', 'J', 'Q', 'K', 'A', 'f' };


        public static int[] GetTypeCount(Card[] pack)
        {
            //returns an array with how many cards of each type are in pack
            int[] type_count = { 0, 0, 0, 0 };
            foreach (Card card in pack)
            {
                if (card.GetCardType() == "SPADES")
                    type_count[0]++;
                else if (card.GetCardType() == "CLUBS")
                    type_count[1]++;
                else if (card.GetCardType() == "DIAMONDS")
                    type_count[2]++;
                else if (card.GetCardType() == "HEARTS")
                    type_count[3]++;
            }
            return type_count;
        }

        public static int GetStrongCount(Card[] pack, string strong)
        {
            int strong_counter = 0;
            for (int i = 0; i < pack.Length; i++)
            {
                if (pack[i].GetCardType() == strong)
                    strong_counter++;
            }
            return strong_counter;
        }


        static int GetPower(string type)
        {
            char pow = type[type.Length - 1];
            for (int i = 0; i < TYPES.Length; i++)
                if (TYPES[i] == pow)
                    return i;
            return -1;
        }

        public static Card EducatedRandomNoStrong(Card[] pack, string strong)
        {
            Card selected = pack[0];
            foreach (Card card in pack) //make sure we dont return trump card
                if (card.GetCardType() != strong)
                {
                    selected.SetType(card.GetCardType());
                    selected.SetRank(card.GetCardRank());
                }
            foreach (Card card in pack) // return lowest ranking card thats not a trump card
                if (card.GetCardType() != strong)
                    if (GetPower(card.GetCardRank()) < GetPower(selected.GetCardRank()))
                    {
                        selected.SetType(card.GetCardType());
                        selected.SetRank(card.GetCardRank());
                    }
            return selected;
        }

        public static Card ChooseCard(int counter, string suit, string strong, int id, int partner_id, Card[] pack, Card[] played_cards, string[] discover)
        {
            Card selected = pack[0];

            int[] type_count = GetTypeCount(pack);
            int max = 0;
            for (int i = 0; i < 4; i++)
            {
                if (type_count[i] > max)
                {
                    max = type_count[i];
                    most_common = types[i];
                }
            }

            Card minCard = new Card(most_common, "rank_A");
            for (int i = 0; i < pack.Length; i++)
            {
                if (pack[i].GetValue() < minCard.GetValue())
                    minCard = pack[i];
            }

            for (int i = 0; i < played_cards.Length; i++)
            {
                if (played_cards[i].GetCardType() == strong && GetStrongCount(pack, strong) == 0) //if strong played and i have none
                    return minCard;
            }

            Card first_card = played_cards[Strategy.GetOrder(counter)[0]];
            int current_winner = Strategy.GetCurrentWinner(played_cards, counter);
            bool have_type = false;
            for (int i = 0; i < pack.Length; i++)
            {
                if (first_card.GetCardType() == pack[i].GetCardType())
                    have_type = true;
            }
            if (current_winner == partner_id && !have_type) //if partner is winning and pack doesnt contain the type
            {
                selected = EducatedRandomNoStrong(pack, strong);
            }

            int[] ord = Strategy.GetOrder(counter);
            int myturn = Array.IndexOf(ord, id) + 1;
            int partner_turn = Array.IndexOf(ord, partner_id) + 1;
            if (myturn != 1)
            {
                int[] kills = new int[4];
                for (int i = 0; i < 4; i++)
                    kills[i] = 0;
                for (int i = 0; i < played_cards.Length; i++)
                {
                    if (first_card.GetCardType() != played_cards[i].GetCardType() && first_card.GetCardType() != strong)
                        kills[i] = 1;
                }
                int last_killer = Array.LastIndexOf(kills, 1) + 1;
                if (last_killer == partner_turn) //if partner was the last to kill
                    selected = EducatedRandomNoStrong(pack, strong);
            }

            return selected;
        }
    }
}
