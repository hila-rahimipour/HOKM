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
        public void SetType(string type)
        {
            this.type = type;
        }
        public void SetRank(string rank)
        {
            this.rank = rank;
        }
        public void SetPlayer(int player)
        {
            this.player = player;
        }
        public void SetTurn(int turn)
        {
            this.turn = turn;
        }
    }
}
