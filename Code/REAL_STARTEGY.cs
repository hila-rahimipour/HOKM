using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace HOKM.Code
{
    public class REAL_STARTEGY
    {
        private static int ID = -1;
        private static int partner_id;

        private static Card[] pack;
        private static string strong;

        private static string[] CARD_TYPES = { "SPADES", "CLUBS", "DIAMONDS", "HEARTS" };
        private static string[] discover = new string[4];
        private static int[] big_card = { 13, 13, 13, 13 };


        /// <summary>
        /// Gets the first five cards, then returns the strong suit if we are the rulers.
        /// </summary>
        /// <param name="sock"> The socket to communicate with the server. </param>
        public static string GetStrong(Socket sock)
        {
            // Retrieving the ruler's ID.
            string mes = Networking.RecvMessage(sock);
            int ruler_id = int.Parse(mes.Split(':')[1]);

            // Getting the first five cards.
            mes = Networking.RecvMessage(sock);
            string[] five_string_arr = mes.Split('|');
            Card[] first_five = new Card[5];
            for (int i = 0; i < 5; i++)
                first_five[i] = new Card(five_string_arr[i].Split('*')[0], five_string_arr[i].Split('*')[1]);

            if (ruler_id == ID)
            {
                int[] types_count = new int[4];
                int[] sum_cards = new int[4];
                int index = 0;

                // Finding the sum of all card values and the card amount of each suit.
                foreach (Card card in first_five)
                {
                    for (int i = 0; i < 4; i++)
                        if (CARD_TYPES[i] == card.GetCardType())
                        {
                            index = i;
                            break;
                        }
                    types_count[index]++;
                    sum_cards[index] += card.GetValue();
                }

                int max = 0;
                index = 0;
                // Finding the suit for which the multiplication of the sum and count are the biggest. 
                for (int i = 0; i < 4; i++)
                    if (types_count[i] * sum_cards[i] > max)
                    {
                        index = i;
                        max = types_count[i] * sum_cards[i];
                    }

                return CARD_TYPES[index];
            }
            return null;
        }

        /// <summary>
        /// Getting the order of the players in this round.
        /// </summary>
        /// <param name="counter"> My turn number. </param>
        public static int[] GetOrder(int counter)
        {
            int[] order = { 1, 3, 2, 4 };
            int[] result = new int[4];
            for (int i = 0; i < 4; i++)
                result[i] = order[(counter - 1 + i) % 4];
            return result;
        }

        /// <summary>
        /// Analyzing the last round to learn of others' cards.
        /// </summary>
        /// <param name="counter"> My turn number. </param>
        /// <param name="played_cards"> The cards that were played in this round. </param>
        public static void Discover(int counter, Card[] played_cards)
        {
            // Updating the biggest non-played card.
            for (int i = 0; i < 4; i++)
                foreach (Card card in played_cards)
                    for (int k = 0; k < CARD_TYPES.Length; k++)
                        if (card.GetCardType() == CARD_TYPES[k] && card.GetValue() == big_card[k])
                            big_card[k]--;

            int[] order = GetOrder(counter);

            // Checking who has no cards of the current suit, and who has no strong cards.

            Card first_card = played_cards[order[0] - 1];
            for (int i = 0; i < played_cards.Length; i++)
                if (first_card.GetCardType() != played_cards[i].GetCardType())
                    discover[i] = discover[i] + "KILL " + first_card.GetCardType() + "|";

            if (first_card.GetCardType() == strong)
                for (int i = 0; i < played_cards.Length; i++)
                    if (strong != played_cards[i].GetCardType())
                        discover[i] = discover[i] + "NO STRONG|";


            Card winner_card = GetCurrentWinnerCard(played_cards);
            Card partner_card = played_cards[partner_id - 1];

            int first_player = order[0];
            if (first_player != partner_id)
            {
                if (winner_card.GetCardType() == first_card.GetCardType())
                    if (partner_card.GetCardType() == winner_card.GetCardType())
                        if (winner_card.GetValue() - partner_card.GetValue() < 3)
                            discover[partner_id] = discover[partner_id] + partner_card.GetCardType() + "|";
            }
        }

        /// <summary>
        /// Killing with our smallest card.
        /// </summary>
        /// <param name="counter"> My turn number. </param>
        /// <param name="played_cards"> The cards that were played in this round. </param>
        /// <param name="suit"> The current suit. </param>
        public static Card KillSmall(string suit, int counter, Card[] played_cards)
        {
            // Checking if I have the current suit.
            bool have_type = false;
            foreach (Card card in pack)
                if (card.GetCardType() == suit)
                {
                    have_type = true;
                    break;
                }

            // Checking who killed with the higher value.
            bool enemy_killed = false;
            Card killer = null;
            for (int i = 0; i < played_cards.Length; i++)
            {
                if (played_cards[i] == null)
                    continue;
                if (played_cards[i].GetCardType() != suit && played_cards[i].GetCardType() == strong)
                {
                    if (killer == null)
                        killer = played_cards[i];
                    else if (played_cards[i].GetValue() > killer.GetValue())
                        killer = played_cards[i];
                    if (i + 1 != partner_id)
                        enemy_killed = true;
                }
            }
            
            // Killing with our smallest possible card, or using our smallest card overall.
            int strong_counter = 0;
            Card my_card = new Card("DIAMONDS", "f");  // Placeholder
            if (!have_type && GetCurrentWinner(played_cards) != partner_id)
            {
                if (enemy_killed)
                {
                    foreach (Card card in pack)
                        if (card.GetCardType() == strong)
                        {
                            strong_counter++;
                            if (card.GetValue() > killer.GetValue() && card.GetValue() < my_card.GetValue())
                                my_card = card;
                        }
                }
                else
                {
                    foreach (Card card in pack)
                        if (card.GetCardType() == strong)
                        {
                            strong_counter++;
                            if (card.GetValue() < my_card.GetValue())
                                my_card = card;
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

        /// <summary>
        /// Check if our partner has killed, and use a card so he can kill.
        /// </summary>
        public static Card IfPartnerKillsSomething()
        {
            string[] partner_discover = discover[partner_id - 1].Split('|');
            if (partner_discover.Contains("NO STRONG"))
                return null;

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
                                my_card = card;

            if (my_card.GetCardRank() != "f")
                return my_card;
            else
                return null;
            //use other algorithm or put random card
        }

        /// <summary>
        /// Get the current winner in the round.
        /// </summary>
        /// <param name="played_cards"> The cards played. </param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the winning card in this round.
        /// </summary>
        /// <param name="played_cards"> The cards played. </param>
        public static Card GetCurrentWinnerCard(Card[] played_cards)
        {
            return played_cards[GetCurrentWinner(played_cards) - 1];
        }
    }
}
