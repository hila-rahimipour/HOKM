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
        private static string[] discover = { "", "", "", "" };
        private static int[] big_card = { 13, 13, 13, 13 };


        /// <summary>
        /// The logic for the client.
        /// </summary>
        /// <param name="ID"> Our player ID. </param>
        /// <param name="partner_id"> Our friend's ID. </param>
        /// <param name="suit"> The current suit. </param>
        /// <param name="strong"> The strong suit. </param>
        /// <param name="my_points"> My point count. </param>
        /// <param name="enemy_points"> The enemies' point count. </param>
        /// <param name="played_cards"> The played cards in this turn. </param>
        /// <param name="counter"> My turn number (0-3). </param>
        /// <param name="pack"> The card pack. </param>
        /// <param name="strongCount"> The strong cards played in this match. </param>
        /// <returns></returns>
        public static Card DoTurn(int ID, int partner_id, string suit, string strong, int my_points, int enemy_points, Card[] played_cards, int counter, Card[] pack, List<Card> strongCount)
        {
            int highest = 0; //highest played card in this round
            foreach (Card card in played_cards)
                if (card != null && card.GetCardType() == suit && card.GetValue() > highest)
                    highest = card.GetValue();

            int lowest = 14; // lowest played card in this round.
            foreach (Card card in played_cards)
                if (card != null && card.GetCardType() == suit && card.GetValue() < lowest)
                    lowest = card.GetValue();

            Card lowestCard = new Card("", "f"); // my lowest card.
            Card highestCard = new Card("", "e"); // my highest card.
            foreach (Card card in pack)
                if (card != null && card.GetCardType() == suit && card.GetValue() < lowestCard.GetValue())
                    lowestCard = card;
                else if (card != null && card.GetCardType() == suit && card.GetValue() > highestCard.GetValue())
                    highestCard = card;

            bool have_type = false; // Do I have the type in this turn.
            foreach (Card card in pack)
                if (card != null && card.GetCardType() == suit)
                {
                    have_type = true;
                    break;
                }

            int[] order = GetOrder(counter); // order of the game players

            bool enemy_killed = false;  // Did an enemy kill.
            Card killer = null;  // The highest card that killed in this round.
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
            string[] kills;
            if (partner_discover.Length < 5)
                kills = new string[5];
            else
                kills = new string[partner_discover.Length];
            for (int i = 0; i < partner_discover.Length; i++)
                if (partner_discover[i].Contains("KILL"))
                    kills[i] = partner_discover[i].Substring(5);

            int strongFromTop = CountTopStrong(strong, pack, strongCount);

            Card selected = new Card("", "f");

            // Checking if we can win simply by putting our highest strong cards, or if the enemies are about to win.
            // Then, we put out our highest strong card.
            if (strongFromTop >= 7 - my_points || enemy_points >= 5)
                if (suit == "" || !have_type)
                {
                    Card highestStrong = new Card("", "e");
                    foreach (Card card in pack)
                        if (card != null && card.GetCardType() == strong && card.GetValue() > highestStrong.GetValue())
                            highestStrong = card;
                    if (highestStrong.GetCardRank() != "e")
                        return highestStrong;
                }

            // We play first.
            if (counter == 0)
            {
                bool isKill = false;
                // Checking if we can put the highest card of a suit without anyone killing it.
                for (int i = 0; i < CARD_TYPES.Length; i++)
                {
                    if (CARD_TYPES[i] == strong)
                        continue;
                    for (int k = 0; k < 4; k++)
                        if (discover[k].Contains("KILL " + CARD_TYPES[i]) && k != partner_id - 1 && k != ID - 1)
                        {
                            isKill = true;
                            break;
                        }
                    if (!isKill)
                        foreach (Card card in pack)
                            if (card != null && card.GetCardType() == CARD_TYPES[i] && card.GetValue() == big_card[i])
                                return card;
                }

                for (int i = 0; i < CARD_TYPES.Length; i++)
                {
                    // Checking if we can get our friend to kill a card and win.
                    if (!partner_discover.Contains("NO STRONG"))
                    {
                        selected = new Card("", "f");
                        foreach (Card card in pack)
                            if (kills[i] != "")
                                if (card != null && card.GetCardType() == kills[i])
                                    if (card.GetValue() < selected.GetValue())
                                        selected = card;
                        if (selected.GetCardRank() != "f")
                            return selected;
                    }
                }

                selected = EducatedRandomNoStrong(pack, strong);
            }

            // We play last.
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
                            if (card != null &&
                                card.GetCardType() == suit &&
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
                    if (GetCurrentWinner(strong, played_cards, counter) == partner_id) // if the current winner is our partner
                        selected = EducatedRandomNoStrong(pack, strong); // select an educated random card that isnt a trump card
                    else
                    {
                        if (enemy_killed) // if our enemy killed in this round
                        {
                            // find smallest card that can beat the killer:
                            foreach (Card card in pack)
                                if (card != null && card.GetCardType() == strong) // if we have a strong card 
                                    if (card.GetValue() > killer.GetValue() && card.GetValue() < selected.GetValue()) //if our strong card has a higher value than the current winner but its value is lower than the selected card
                                        selected = card; // return lowest strong card
                            if (selected.GetCardRank() == "f")
                                selected = EducatedRandomNoStrong(pack, strong);
                        }
                        else
                        {
                            if (GetStrongCount(pack, strong) != 0)
                                selected = EducatedRandomSuit(pack, strong);
                            else
                                selected = EducatedRandomNoStrong(pack, strong);
                        }
                    }
                }
            }

            else  // Second or third play of the round.
            {
                if (have_type)

                {   // If we have the highest card of said suit: play it.
                    foreach (Card card in pack)
                        if (card != null && big_card[Array.IndexOf(CARD_TYPES, card.GetCardType())] == card.GetValue() && card.GetCardType() == suit && suit != strong)
                            return card;

                    int nextPlayer = order[(Array.IndexOf(order, ID) + 1) % 4] - 1;
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
                            // if 3rd turn: put a higher card than the winning card to make the final one kill (or make us win).
                            // if 2nd turn: if kill and no strong or no kill: put higher than winning card. if kill and strong: put our lowest card.
                            if (counter == 2)
                            {
                                int min = 14;
                                foreach (Card card in pack)
                                    if (card != null &&
                                        card.GetCardType() == suit &&
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
                                        if (card != null &&
                                            card.GetCardType() == suit &&
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

                else  // we don't have the type.
                {

                    if (GetCurrentWinner(strong, played_cards, counter) == partner_id) // if the current winner is our partner
                        return EducatedRandomNoStrong(pack, strong); // return a random low value card that isnt a trump card
                    else
                    {
                        if (enemy_killed)
                        {
                            // try to kill the killer.
                            int min = 14;
                            foreach (Card card in pack)
                                if (card != null &&
                                    card.GetCardType() == strong &&
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
                            // kill with our lowest card.
                            int min = 14;
                            foreach (Card card in pack)
                                if (card != null &&
                                    card.GetCardType() == strong &&
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

        /// <summary>
        /// Gets the amount of highest strong cards we have (e.g. A > K > Q > J...)
        /// </summary>
        /// <param name="strong"> The strong suit </param>
        /// <param name="pack"> Our pack. </param>
        /// <param name="playedStrong"> The played strong cards. </param>
        /// <returns></returns>
        static int CountTopStrong(string strong, Card[] pack, List<Card> playedStrong)
        {
            int counter = 0;
            Card temp = new Card(strong, big_card[Array.IndexOf(CARD_TYPES, strong)]);

            int[] playedValues = new int[playedStrong.Count];
            for (int i = 0; i < playedStrong.Count; i++)
                playedValues[i] = playedStrong[i].GetValue();

            foreach (Card card in pack)
            {
                if (card != null && card.GetCardType() == strong && card.GetValue() == temp.GetValue())
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
                result[i] = order[(counter + i) % 4];
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
                if (first_card.GetCardType() != played_cards[i].GetCardType() && !discover.Contains("KILL " + first_card.GetCardType()))
                    discover[i] = discover[i] + "KILL " + first_card.GetCardType() + "|";

            if (first_card.GetCardType() == strong)
                for (int i = 0; i < played_cards.Length; i++)
                    if (strong != played_cards[i].GetCardType() && !discover.Contains("NO STRONG"))
                        discover[i] = discover[i] + "NO STRONG|";


            Card winner_card = GetCurrentWinnerCard(strong, played_cards, counter);
            Card partner_card = played_cards[partner_id - 1];

            int first_player = order[0];
            if (first_player != partner_id && counter != 0)
                if (winner_card.GetCardType() == first_card.GetCardType())
                    if (partner_card.GetCardType() == winner_card.GetCardType())
                        if (winner_card.GetValue() - partner_card.GetValue() < 3 && !discover.Contains("KILL " + partner_card.GetCardType()))
                            discover[partner_id - 1] = discover[partner_id - 1] + "KILL " + partner_card.GetCardType() + "|";
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
            if (current_winner_card == null)
                return -1;
            int current_winner_id = 1;
            foreach (int i in order)
            {
                if (played_cards[i - 1] == null)
                    continue;
                if ((current_winner_card.GetValue() < played_cards[i - 1].GetValue() &&
                    current_winner_card.GetCardType() == played_cards[i - 1].GetCardType())
                    ||
                    (played_cards[i - 1].GetCardType() == strong &&
                    current_winner_card.GetCardType() != strong))
                {
                    current_winner_card = played_cards[i - 1];
                    current_winner_id = i;
                }
            }
            return current_winner_id;
        }

        /// <summary>
        /// Gets the winning card in this round.
        /// </summary>
        /// <param name="played_cards"> The cards played. </param>
        public static Card GetCurrentWinnerCard(string strong, Card[] played_cards, int counter)
        {
            int winner = GetCurrentWinner(strong, played_cards, counter);
            if (winner == -1)
                return null;
            return played_cards[winner - 1];
        }

        public static int GetStrongCount(Card[] pack, string strong)
        {
            int strong_counter = 0;
            for (int i = 0; i < pack.Length; i++)
            {
                if (pack[i] != null && pack[i].GetCardType() == strong)
                    strong_counter++;
            }
            return strong_counter;
        }

        /// <summary>
        /// Gets the lowest card in the pack which isn't a strong card.
        /// </summary>
        /// <param name="pack"> The card pack. </param>
        /// <param name="strong"> The strong suit. </param>
        /// <returns></returns>
        public static Card EducatedRandomNoStrong(Card[] pack, string strong)
        {
            // lowest card that isn't strong
            Card selected = pack[0];
            for (int i = 0; i < 13; i++)
                if (pack[i] != null)
                    selected = pack[i];

            foreach (Card card in pack) //make sure we dont return trump card
                if (card != null && card.GetCardType() != strong)
                    selected = card;
            foreach (Card card in pack) // return lowest ranking card thats not a trump card
                if (card != null && card.GetCardType() != strong)
                    if (card.GetValue() < selected.GetValue())
                        selected = card;
            return selected;
        }

        /// <summary>
        /// Gets the lowest card in the pack of a certain suit.
        /// </summary>
        /// <param name="pack"> The card pack. </param>
        /// <param name="suit"> The suit. </param>
        /// <returns></returns>
        public static Card EducatedRandomSuit(Card[] pack, string suit)
        {
            // lowest card that isn't strong
            Card selected = pack[0];
            for (int i = 0; i < 13; i++)
                if (pack[i] != null)
                    selected = pack[i];

            foreach (Card card in pack) //make sure we dont return trump card
                if (card != null && card.GetCardType() == suit)
                    selected = card;
            foreach (Card card in pack) // return lowest ranking card thats not a trump card
                if (card != null && card.GetCardType() == suit)
                    if (card.GetValue() < selected.GetValue())
                        selected = card;
            return selected;
        }
    }
}
