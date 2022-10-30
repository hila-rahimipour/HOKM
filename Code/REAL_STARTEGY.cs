using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace HOKM.Code
{
    internal class REAL_STARTEGY
    {
        private static int ID = -1;
        private static int partner_id;

        private static Card[] pack;
        private static string strong;

        private static string[] discover = new string[4];
        private static int[] big_card = { 13, 13, 13, 13 };


        public static string GetStrong(Socket sock)
        {
            string mes = Networking.RecvMessage(sock);
            int ruler_id = int.Parse(mes.Split(':')[1]);

            mes = Networking.RecvMessage(sock);
            string[] five_string_arr = mes.Split('|');
            Card[] first_five = new Card[5];
            for (int i = 0; i < 5; i++)
                first_five[i] = new Card(five_string_arr[i].Split('*')[0], five_string_arr[i].Split('*')[1]);

            if (ruler_id == ID)
            {
                Console.WriteLine("I am the ruler");

                //0-SPADES, 1-CLUBS, 2-DIAMONDS, 3-HEARTS
                int[] types_count = new int[4];
                int[] sum_cards = new int[4];

                for (int i = 0; i < first_five.Length; i++)
                    switch (first_five[i].GetCardType())
                    {
                        case "SPADES":
                            types_count[0]++;
                            sum_cards[0] += first_five[i].GetValue();
                            break;
                        case "CLUBS":
                            types_count[1]++;
                            sum_cards[1] += first_five[i].GetValue();
                            break;
                        case "DIAMONDS":
                            types_count[2]++;
                            sum_cards[2] += first_five[i].GetValue();
                            break;
                        case "HEARTS":
                            types_count[3]++;
                            sum_cards[3] += first_five[i].GetValue();
                            break;
                    }

                int max = 0;
                int index = 0;
                for (int i = 0; i < 4; i++)
                    if (types_count[i] * sum_cards[i] > max)
                    {
                        index = i;
                        max = types_count[i] * sum_cards[i];
                    }

                string[] names = { "SPADES", "CLUBS", "DIAMONDS", "HEARTS" };
                return names[index];
            }
            return null;
        }

        public static int[] GetOrder(int counter)
        {
            int[] order = new int[4] { 1, 3, 2, 4 };
            int[] result = new int[4];
            for (int i = 0; i < 4; i++)
                result[i] = order[(counter - 1 + i) % 4];
            return result;
        }

        public static void Discover(int counter, Card[] played_cards)
        {
            // Analyzes the round after its end.

            // Updating the biggest non-played card:
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

            int[] order = GetOrder(counter);

            Card first_card = played_cards[order[0] - 1];
            for (int i = 0; i < played_cards.Length; i++)
                if (first_card.GetCardType() != played_cards[i].GetCardType())
                    discover[i] = discover[i] + "KILL " + first_card.GetCardType() + "|";

            if (first_card.GetCardType() == strong)
                for (int i = 0; i < played_cards.Length; i++)
                    if (strong != played_cards[i].GetCardType())
                        discover[i] = discover[i] + "NO STRONG|";

            Card winner_card = GetCurrentWinnerCard(played_cards);

            int first_player = order[0];
            if (first_player != partner_id)
            {
                Card partner_card = played_cards[partner_id - 1];
                if (winner_card.GetCardType() == first_card.GetCardType())
                    if (partner_card.GetCardType() == winner_card.GetCardType())
                        if (winner_card.GetValue() - partner_card.GetValue() < 3)
                            discover[partner_id] = discover[partner_id] + "KILL " + partner_card.GetCardType() + "|";
            }
        }

        public static Card killSmall(int counter, Card[] played_cards)
        {
            int beginner = GetOrder(counter)[0];
            Card first_card = played_cards[beginner - 1];
            bool have_type = false;
            for (int i = 0; i < pack.Length; i++)
                if (first_card.GetCardType() == pack[i].GetCardType())
                    have_type = true;

            bool enemy_killed = false;
            Card killer = null;
            for (int i = 0; i < played_cards.Length; i++)
            {
                if (first_card.GetCardType() != played_cards[i].GetCardType() && played_cards[i].GetCardType() == strong)
                {
                    if (killer == null)
                        killer = played_cards[i];
                    else if (killer.GetValue() < played_cards[i].GetValue())
                        killer = played_cards[i];
                    if (i + 1 != partner_id)
                        enemy_killed = true;
                }
            }

            int strong_counter = 0;
            Card my_card = new Card("DIAMONDS", "f");  // Placeholder
            if (!have_type && GetCurrentWinner(played_cards) != partner_id)
            {
                if (enemy_killed)
                {
                    for (int i = 0; i < pack.Length; i++)
                        if (pack[i].GetCardType() == strong)
                        {
                            strong_counter++;
                            if (pack[i].GetValue() > killer.GetValue() && pack[i].GetValue() < my_card.GetValue())
                                my_card = pack[i];
                        }
                }
                else
                {
                    for (int i = 0; i < pack.Length; i++)
                        if (pack[i].GetCardType() == strong)
                        {
                            strong_counter++;
                            if (pack[i].GetValue() < my_card.GetValue())
                                my_card = pack[i];
                        }
                }
            }
            if (strong_counter >= 4 && my_card.GetValue() < 10)
                return my_card;
            else
            {
                Card minCard = new Card("DIAMONDS", "f");
                for (int i = 0; i < pack.Length; i++)
                {
                    if (pack[i].GetValue() < minCard.GetValue())
                        minCard = pack[i];
                }
                return minCard;
            }
        }

        public static Card IfPartnerKillsSomething()
        {
            string[] partner_discover = discover[partner_id - 1].Split('|');
            string[] kills = new string[4];
            for (int i = 0; i < partner_discover.Length; i++)
                if (partner_discover[i].Contains("KILL"))
                    kills[i] = partner_discover[i].Substring(5);

            Card my_card = new Card("DIAMONDS", "f");
            foreach (Card card in pack)
                for (int j = 0; j < kills.Length; j++)
                    if (kills[j] != null)
                        if (card.GetCardType() == kills[j])
                            if (card.GetValue() < my_card.GetValue())
                                my_card = card;  // Add check if he has strong

            if (my_card.GetCardRank() != "f")
                return my_card;
            else
                return null;
            //use other algorithem or put random card
        }

        public static int GetCurrentWinner(Card[] played_cards)
        {
            Card current_winner_card = played_cards[0];
            int current_winner_id = 1;
            for (int i = 0; i < played_cards.Length; i++)
                if ((current_winner_card.GetValue() < played_cards[i].GetValue() &&
                    current_winner_card.GetCardType() == played_cards[i].GetCardType())
                    ||
                    (played_cards[i].GetCardType() == strong &&
                    current_winner_card.GetCardType() != strong))
                {
                    current_winner_card = played_cards[i];
                    current_winner_id = i + 1;
                }
            return current_winner_id;
        }

        public static Card GetCurrentWinnerCard(Card[] played_cards)
        {
            return played_cards[GetCurrentWinner(played_cards) - 1];
        }
    }
}
