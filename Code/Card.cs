using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOKM.Code
{
    public class Card
    {

        static string[] TYPES = { "e", "rank_2", "rank_3", "rank_4", "rank_5", "rank_6", "rank_7", "rank_8",
                                    "rank_9", "rank_10", "rank_J", "rank_Q", "rank_K", "rank_A", "f"};


        private string type;
        private string rank;

        public Card(string type, string rank)
        {
            this.type = type;
            this.rank = rank;
        }

        public Card(string type, int rank)
        {
            this.type = type;
            this.rank = TYPES[rank];
        }

        public string GetCardType() => type;
        public string GetCardRank() => rank;
        public void SetType(string type)
        {
            this.type = type;
        }
        public void SetRank(string rank)
        {
            this.rank = rank;
        }
        public int GetValue()
        {
            if (TYPES.Contains(type))
                return Array.IndexOf(TYPES, type);
            return -1;
        }
    }
}
