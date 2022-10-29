using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOKM.Code
{
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
                if(card.GetCardType() != strong)
                    if (GetPower(card.GetCardRank()) < GetPower(selected.GetCardRank()))
                    {
                        selected.SetType(card.GetCardType());
                        selected.SetRank(card.GetCardRank());
                    }
            return selected;
        }

        public static Card ChooseCard(int counter, string suit, string strong, int id,  int partner_id, Card[] pack, Card[] played_cards, string[] discover)
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

            Card first_card = played_cards[Test.GetOrder(counter)[0]];
            int current_winner = Test.GetCurrentWinner(played_cards, counter);
            bool have_type = false;
            for (int i = 0; i < pack.Length; i++)
            {
                if (first_card.GetCardType() == pack[i].GetCardType())
                    have_type = true;
            }
            if (current_winner == partner_id && !have_type) //if partner is winning and pack doesnt contain the type
            {
                selected = EducatedRandomNoStrong(pack,strong);
            }

            int[] ord = Test.GetOrder(counter);
            int myturn = Array.IndexOf(ord, id)+1;
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
                if(last_killer == partner_turn) //if partner was the last to kill
                    selected = EducatedRandomNoStrong(pack, strong);
            }

            return selected;
        }
    }
}
