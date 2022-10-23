using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOKM.Code
{
    internal class Strategy
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

        public static Card GetCard(string suit, Card[] pack, Card[] played, List<Card> memory)
        {

            // Finding the highest card played in this suit:
            string highest = "e";

            foreach (Card card in memory)     
                if (card.GetCardType() == suit && GetPower(card.GetCardRank()) > GetPower(highest))
                    highest = card.GetCardRank();

            foreach (Card card in played)
                if (card.GetCardType() == suit && GetPower(card.GetCardRank()) > GetPower(highest))
                    highest = card.GetCardRank();

            if (highest == "e")
                highest = "f";

            // Checking if I have the highest card of said suit:
            Card selected = new Card(suit, "f");
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

            return selected;
        }
    }
}
