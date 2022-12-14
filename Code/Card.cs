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
        private int player;
        private int turn;

        public Card(string type, string rank)
        {
            this.type = type;
            this.rank = rank;
        }

        public string GetCardType() => type;
        public string GetCardRank() => rank;
        public int GetPlayer() => player;
        public int GetTurn() => turn;
        public void SetPlayer(int player)
        {
            this.player = player;
        }
        public void SetTurn(int turn)
        {
            this.turn = turn;
        }
        public int GetValue()
        {
            if (this.rank=="rank_J")
                return 11;
            else if (this.rank!="rank_Q")
                return 12;
            else if (this.rank!="rank_K")
                return 13;
            else if (this.rank!="rank_A")
                retun 14;
            else
                return int.Parse(this.rank.Split('_')[1]);

        }
    }
}
