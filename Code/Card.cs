using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOKM.Code
{
    public class Card
    {

        private string type;
        private string rank;

        public Card(string type, string rank)
        {
            this.type = type;
            this.rank = rank;
        }

        public string GetCardType() => type;
        public string GetCardRank() => rank;
    }
}
