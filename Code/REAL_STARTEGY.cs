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

        private static string[] CARD_TYPES = { "SPADES", "CLUBS", "DIAMONDS", "HEARTS" };
        private static string[] discover = new string[4];
        private static int[] big_card = { 13, 13, 13, 13 };


        public static Card DoTurn(int ID, int partner_id, string suit, string strong, int my_points, int enemy_points, Card[] played_cards, int counter, Card[] pack, List<Card> strongCount)
        {
            int highest = 0;
            foreach (Card card in played_cards)
                if (card.GetCardType() == suit && card.GetValue() > highest)
                    highest = card.GetValue();

            int lowest = 14;
            foreach (Card card in played_cards)
                if (card.GetCardType() == suit && card.GetValue() < lowest)
                    lowest = card.GetValue();

            Card lowestCard = new Card("", "f");
            Card highestCard = new Card("", "e");
            foreach (Card card in pack)
                if (card.GetCardType() == suit && card.GetValue() < lowestCard.GetValue())
                    lowestCard = card;
                else if (card.GetCardType() == suit && card.GetValue() > highestCard.GetValue())
                    highestCard = card;

            bool have_type = false;
            foreach (Card card in pack)
                if (card.GetCardType() == suit)
                {
                    have_type = true;
                    break;
                }

            int[] order = GetOrder(counter);

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

            string[] partner_discover = discover[partner_id - 1].Split('|');
            string[] kills = new string[4];
            for (int i = 0; i < partner_discover.Length; i++)
                if (partner_discover[i].Contains("KILL"))
                    kills[i] = partner_discover[i].Substring(5);

            int strongFromTop = CountTopStrong(strong, pack, strongCount);

            Card selected = new Card("", "f");

            if (strongFromTop >= 7 - my_points)
                if (suit == "" || !have_type)
                {
                    Card highestStrong = new Card("", "e");
                    foreach (Card card in pack)
                        if (card.GetCardType() == strong && card.GetValue() > highestStrong.GetValue())
                            highestStrong = card;
                    return highestStrong;
                }


            if (suit == "")
            {
                bool isKill = false;
                for (int i = 0; i < CARD_TYPES.Length; i++)
                {
                    for (int k = 0; k < 4; k++)
                        if (!discover[k].Contains("KILL " + CARD_TYPES[i]) && k != partner_id - 1 && k != ID - 1)
                        {
                            isKill = true;
                            break;
                        }
                    if (!isKill)
                        foreach (Card card in pack)
                            if (card.GetCardType() == CARD_TYPES[i] && card.GetValue() == big_card[i])
                                return card;
                }

                for (int i = 0; i < CARD_TYPES.Length; i++)
                {
                    if (!partner_discover.Contains("NO STRONG"))
                    {
                        selected = new Card("", "f");
                        foreach (Card card in pack)
                            if (kills[i] != "")
                                if (card.GetCardType() == kills[i])
                                    if (card.GetValue() < selected.GetValue())
                                        selected = card;
                        if (selected.GetCardRank() != "f")
                            return selected;
                    }
                }

                selected = EducatedRandomNoStrong(pack, strong);
            }

            else if (counter == 3)
            {
                if (have_type)
                {
                    if (killer != null)  // If someone killed.
                        selected = lowestCard;
                    else
                    {
                        int min = 14;
                        foreach (Card card in pack)
                            if (card.GetCardType() == suit &&
                                card.GetValue() > highest &&
                                card.GetValue() < min)
                            {
                                selected = card;
                                min = card.GetValue();
                            }
                        if (min == 14)
                            selected = lowestCard;
                    }
                }
                else
                {
                    if (GetCurrentWinner(strong, played_cards, counter) == partner_id)
                        selected = EducatedRandomNoStrong(pack, strong);
                    else
                    {
                        if (enemy_killed)
                        {
                            foreach (Card card in pack)
                                if (card.GetCardType() == strong)
                                {
                                    if (card.GetValue() > killer.GetValue() && card.GetValue() < selected.GetValue())
                                        selected = card;
                                }
                            if (selected.GetCardRank() == "f")
                                selected = EducatedRandomNoStrong(pack, strong);
                        }
                        else
                        {
                            int min = 14;
                            foreach (Card card in pack)
                                if (card.GetCardType() == strong &&
                                    card.GetValue() < min)
                                {
                                    selected = card;
                                    min = card.GetValue();
                                }
                            if (min == 14)
                                selected = EducatedRandomNoStrong(pack, strong);
                        }
                    }
                }
            }

            else
            {
                if (have_type)
                {
                    foreach (Card card in pack)
                        if (big_card[Array.IndexOf(CARD_TYPES, card.GetCardType())] == card.GetValue() && card.GetCardType() == suit && suit != strong)
                            return card;

                    int nextPlayer = order[Array.IndexOf(order, ID) + 1] - 1;
                    string[] data = discover[nextPlayer].Split('|');
                    bool killSuit = false;
                    bool noStrong = false;
                    for (int i = 0; i < data.Length - 1; i++)
                    {
                        if (data[i].StartsWith("KILL") && data[i].Substring(5) == suit)
                            killSuit = true;
                        else if (data[i] == "NO STRONG")
                            noStrong = true;
                    }

                    if (GetCurrentWinner(strong, played_cards, counter) == partner_id)
                        return EducatedRandomSuit(pack, suit);
                    else
                    {
                        if (enemy_killed)
                            return EducatedRandomSuit(pack, suit);
                        else
                        {
                            // if 3rd turn put higher card than winning to make him kill
                            // if 2nd turn if kill and no strong or no kill put higher if kill and strong lowest
                            if (counter == 2)
                            {
                                int min = 14;
                                foreach (Card card in pack)
                                    if (card.GetCardType() == suit &&
                                        card.GetValue() > highest &&
                                        card.GetValue() < min)
                                    {
                                        selected = card;
                                        min = card.GetValue();
                                    }
                                if (min == 14)
                                    selected = EducatedRandomSuit(pack, suit);
                            }
                            else if (counter == 1)
                            {
                                if (killSuit && noStrong || !killSuit)
                                {
                                    int min = 14;
                                    foreach (Card card in pack)
                                        if (card.GetCardType() == suit &&
                                            card.GetValue() > highest &&
                                            card.GetValue() < min)
                                        {
                                            selected = card;
                                            min = card.GetValue();
                                        }
                                    if (min == 14)
                                        selected = EducatedRandomSuit(pack, suit);
                                }
                                else if (killSuit)
                                    selected = EducatedRandomSuit(pack, suit);
                            }
                        }
                    }
                }

                else
                {
                    if (GetCurrentWinner(strong, played_cards, counter) == partner_id)
                        return EducatedRandomNoStrong(pack, strong);
                    else
                    {
                        if (enemy_killed)
                        {
                            int min = 14;
                            foreach (Card card in pack)
                                if (card.GetCardType() == strong &&
                                    card.GetValue() > killer.GetValue() &&
                                    card.GetValue() < min)
                                {
                                    selected = card;
                                    min = card.GetValue();
                                }
                            if (min == 14)
                                selected = EducatedRandomNoStrong(pack, strong);
                        }
                        else
                        {
                            int min = 14;
                            foreach (Card card in pack)
                                if (card.GetCardType() == strong &&
                                    card.GetValue() < min)
                                {
                                    selected = card;
                                    min = card.GetValue();
                                }
                            if (min == 14)
                                selected = EducatedRandomNoStrong(pack, strong);
                        }
                    }
                }
            }

            if (selected == null || !pack.Contains(selected))
                selected = EducatedRandomNoStrong(pack, strong);
            return selected;
        }

        static int CountTopStrong(string strong, Card[] pack, List<Card> playedStrong)
        {
            int counter = 0;
            Card temp = new Card(strong, big_card[Array.IndexOf(CARD_TYPES, strong)]);

            int[] playedValues = new int[playedStrong.Count];
            for (int i = 0; i < playedStrong.Count; i++)
                playedValues[i] = playedStrong[i].GetValue();

            foreach (Card card in pack)
            {
                if (card.GetCardType() == strong && card.GetValue() == temp.GetValue())
                {
                    counter++;
                    while (playedValues.Contains(temp.GetValue()))
                    {
                        if (temp.GetValue() < 0)
                            return counter;
                        temp = new Card(strong, temp.GetValue() - 1);
                    }
                }
            }

            return counter;
        }


        /// <summary>
        /// Gets the first five cards, then returns the strong suit if we are the rulers.
        /// </summary>
        /// <param name="sock"> The socket to communicate with the server. </param>
        public static string GetStrong(Card[] first_five)
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
        public static void Discover(int partner_id, string strong, int counter, Card[] played_cards, List<Card> playedStrong)
        {
            foreach (Card card in played_cards)
                if (card.GetCardType() == strong)
                    playedStrong.Add(card);

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


            Card winner_card = GetCurrentWinnerCard(strong, played_cards, counter);
            Card partner_card = played_cards[partner_id - 1];

            int first_player = order[0];
            if (first_player != partner_id && counter != 0)
            {
                if (winner_card.GetCardType() == first_card.GetCardType())
                    if (partner_card.GetCardType() == winner_card.GetCardType())
                        if (winner_card.GetValue() - partner_card.GetValue() < 3)
                            discover[partner_id] = discover[partner_id] + partner_card.GetCardType() + "|";
            }
        }

        /// <summary>
        /// Get the current winner in the round.
        /// </summary>
        /// <param name="played_cards"> The cards played. </param>
        /// <returns></returns>
        public static int GetCurrentWinner(string strong, Card[] played_cards, int counter)
        {
            int[] order = GetOrder(counter);
            Card current_winner_card = played_cards[order[0] - 1];
            int current_winner_id = 1;
            foreach (int i in order)
                if ((current_winner_card.GetValue() < played_cards[i - 1].GetValue() &&
                    current_winner_card.GetCardType() == played_cards[i - 1].GetCardType())
                    ||
                    (played_cards[i - 1].GetCardType() == strong &&
                    current_winner_card.GetCardType() != strong))
                {
                    current_winner_card = played_cards[i - 1];
                    current_winner_id = i;
                }
            return current_winner_id;
        }

        /// <summary>
        /// Gets the winning card in this round.
        /// </summary>
        /// <param name="played_cards"> The cards played. </param>
        public static Card GetCurrentWinnerCard(string strong, Card[] played_cards, int counter)
        {
            return played_cards[GetCurrentWinner(strong, played_cards, counter) - 1];
        }


        public static int[] GetTypeCount(Card[] pack)
        {
            //returns an array with how many cards of each type are in pack
            int[] type_count = { 0, 0, 0, 0 };
            int index = 0;
            foreach (Card card in pack)
            {
                for (int i = 0; i < 4; i++)
                    if (CARD_TYPES[i] == card.GetCardType())
                    {
                        index = i;
                        break;
                    }
                type_count[index]++;
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

        public static Card EducatedRandomNoStrong(Card[] pack, string strong)
        {
            // lowest card that isn't strong
            Card selected = pack[0];
            foreach (Card card in pack) //make sure we dont return trump card
                if (card.GetCardType() != strong)
                {
                    selected.SetType(card.GetCardType());
                    selected.SetRank(card.GetCardRank());
                }
            foreach (Card card in pack) // return lowest ranking card thats not a trump card
                if (card.GetCardType() != strong)
                    if (card.GetValue() < selected.GetValue())
                    {
                        selected.SetType(card.GetCardType());
                        selected.SetRank(card.GetCardRank());
                    }
            return selected;
        }

        public static Card EducatedRandomSuit(Card[] pack, string suit)
        {
            // lowest card that isn't strong
            Card selected = pack[0];
            foreach (Card card in pack) //make sure we dont return trump card
                if (card.GetCardType() == suit)
                {
                    selected.SetType(card.GetCardType());
                    selected.SetRank(card.GetCardRank());
                }
            foreach (Card card in pack) // return lowest ranking card thats not a trump card
                if (card.GetCardType() == suit)
                    if (card.GetValue() < selected.GetValue())
                    {
                        selected.SetType(card.GetCardType());
                        selected.SetRank(card.GetCardRank());
                    }
            return selected;
        }
    }
}
