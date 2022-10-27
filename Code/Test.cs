using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Diagnostics.Eventing.Reader;

namespace HOKM.Code
{
    internal class Test
    {

        private static string SERVER_ADDR = "10.0.0.4";
        private static int SERVER_PORT = 55555;
        private static int ID = -1;
        private static string USERNAME = "Name";

        private static Card[] pack;
        private static int partner_id;
        private static string strong;

        private static List<Card> card_history = new List<Card>();
        private static int[] points = new int[2];

        private static int enemy1=0;
        private static int enemy2=0;
        private static string[] discover = new string[5];

        private string int count=-1;

        //0-SPADES, 1-CLUBS, 2-DIAMONDS, 3-HEARTS
        private static int[] big_card = {14, 14, 14, 14};

        public static void Main(string[] args)
        {
            Socket sock = Networking.OpenSocket(SERVER_ADDR, SERVER_PORT);

            // Receive Client ID:
            string id_text = Networking.RecvMessage(sock);
            ID = int.Parse(id_text.Split(':')[1]);
            Console.WriteLine("This client's ID is " + ID);

            // Send Username:
            // Networking.SendMessage(sock, "username:" + USERNAME);

            // Determine Strong:
            strong = GetStrong(sock);
            if (strong != null)
                while (true)
                {
                    Networking.SendMessage(sock, "set_strong:" + strong);
                    if (Networking.RecvMessage(sock) == "ok")
                        break;
                }

            BuildPack(sock);

            bool isGame = true;
            string result;
            int turn;

            // Play:
            while (isGame)
            {
                result = DoTurn(sock);
                turn = int.Parse(result.Split('p')[0]);
                result = result.Substring(1);

                if (result == "GAME_OVER")
                    isGame = false;
                else
                    while (true)
                    {
                        Networking.SendMessage(sock, result);
                        string response = Networking.RecvMessage(sock);
                        if (response == "ok")
                            break;
                        else if (response == "bad_play")
                            result = DoTurn(sock); //do something to change the selected card
                    }
                // Round over:
                string data = Networking.RecvMessage(sock);

                string[] datarr = data.Split(',');

                bool isWinner = datarr[0].Split(':')[1] == (ID + "+" + partner_id) || datarr[0].Split(':')[1] == (partner_id + "+" + ID);

                foreach (string score_data in datarr[1].Split(':')[1].Split('|'))
                {
                    if (score_data.Split('*')[0] == (ID + "+" + partner_id) || score_data.Split('*')[0] == (partner_id + "+" + ID))
                        points[0] = int.Parse(score_data.Split('*')[1]);
                    else
                        points[1] = int.Parse(score_data.Split('*')[1]);
                }

                for (int i = 0; i < 4; i++)
                {
                    string card_data = datarr[2].Split(':')[1].Split('|')[i];
                    Card c = new Card(card_data.Split('*')[0], card_data.Split('*')[1]);
                    c.SetPlayer(i + 1);
                    int cardTurn = (ID + 4 - turn) % 4;
                    if (cardTurn == 2) cardTurn = 3;
                    else if (cardTurn == 3) cardTurn = 2;
                    c.SetTurn(cardTurn);
                    card_history.Add(c);
                }
            }

            Networking.CloseSocket(sock);
        }

        public static string GetStrong(Socket sock)
        {
            string mes = Networking.RecvMessage(sock);
            int ruler_id = int.Parse(mes.Split(':')[1]);
            mes = Networking.RecvMessage(sock);

            if (ruler_id == ID)
            {
                Console.WriteLine("I am the ruler");
                string five_mes = mes;
                string[] five_string_arr = five_mes.Split('|');
                Card[] first_five = new Card[5];
                for (int i = 0; i < 5; i++)
                    first_five[i] = new Card(five_string_arr[i].Split('*')[0], five_string_arr[i].Split('*')[1]);

                //0-SPADES, 1-CLUBS, 2-DIAMONDS, 3-HEARTS
                int[] types_count = {0, 0, 0, 0};
                for (int i=0; i < first_five.Length; i++)
                {
                    switch (first_five[i].GetCardType())
                    {
                        case "SPADES":
                            types_count[0]++;
                        case "CLUBS":
                            types_count[1]++;
                        case "DIAMONDS":
                            types_count[2]++;
                        case "HEARTS":
                            types_count[3]++;
                    }
                }
                int max_cards = types_count[0];
                int strong_id = 0;
                for (int i=1; i < types_count.Length; i++)
                {
                    if (types_count[i] > max_cards)
                    {
                        max_cards = types_count[i];
                        strong_id=i;
                        switch (strong_id)
                        {
                            case 0:
                                strong="SPADES";
                            case 1:
                                strong="CLUBS";
                            case 2:
                                strong = "DIAMONS";
                            case 3:
                                strong = "HEARTS";
                        }
                    }
                }
                for (int i=0; i<types_count.Length; i++)
                {
                    if (max_cards == types_count[i])
                    {
                        int[] max_count_values = new int[2];
                        int count_max = 0;

                        int[] new_max = new int[2];
                        int new_count=0;
                        string new_strong;
                        switch (types_count[i])
                        {
                            case 0:
                                new_strong="SPADES";
                            case 1:
                                new_strong="CLUBS";
                            case 2:
                                new_strong = "DIAMONS";
                            case 3:
                                new_strong = "HEARTS";
                        }

                        for (int j=0; j < first_five.Length; j++)
                        {
                            if (first_five[i].GetCardType() == strong)
                            {
                                max_count_values[count_max] = first_five[i].GetValue();
                                count_max++;
                            }
                            if (first_five[i].GetCardType() == new_strong)
                            {
                                new_max[count_max]=first_five[i].GetValue();
                                count_max++;
                            }
                        }
                        int maxNumFirst = Math.Max(max_count_values[0], max_count_values[1]);
                        int minNumFirst = Math.Min(max_count_values[0], max_count_values[1]);

                        int maxNumSec = Math.Max(new_max[0], new_max[1]);
                        int minNumSec = Math.Min(new_max[0], new_max[1]);
                        if (maxNumFirst>maxNumSec && minNumFirst>minNumSec)
                            return strong;
                        else if (maxNumFirst<maxNumSec && minNumFirst<minNumSec)
                            return new_strong;
                        else
                        {
                            if (maxNumFirst>maxNumSec)
                                return strong;
                            else
                               return new_strong;
                        }

                    }
                }
                ;
            }

            return null;
        }

        public static void BuildPack(Socket sock)
        {
            string mes = Networking.RecvMessage(sock);

            string[] data = mes.Split(',');
            string[] data_cards  = data[0].Split('|');
            string[] teams = data[1].Split(':')[1].Split('|');
            strong = data[2].Split(':')[1];

            pack = new Card[13];
            for (int i=0; i < data_cards.Length; i++)
                pack[i]  =  new Card(data_cards[i].Split('*')[0], data_cards[i].Split('*')[1]);

            foreach (string team in teams) {
                if (team.Contains(ID.ToString()))
                {
                    if (int.Parse(team.Split('+')[0]) == ID)
                        partner_id = int.Parse(team.Split('+')[1]);
                    else
                        partner_id = int.Parse(team.Split('+')[0]);
                }
            }
        }

        public static string DoTurn(Socket sock)
        {
            string mes = Networking.RecvMessage(sock);

            if (mes == "GAME_OVER")
                return "GAME_OVER";
            else if (mes.Contains("round end"))
            {
                Card round_card[] = new Card[4];
                string[] cards = mes.Split('round_cards:')[1].Split('|');
                for (int i=0; i < round_card.Length; i++)
                {
                    round_card[i]=new Card(cards[i].Split('*')[0], cards[i].Split('*')[1]);
                }
                Discover(counter, round_card);


            }

            string[] data = mes.Split(',');
            string suit = data[0].Split(':')[1];
            string[] cards_str = data[1].Split(':')[1].Split('|');
            Card[] played_cards = new Card[cards_str.Length];
            int counter = 0;
            for (int i = 0; i < cards_str.Length; i++)
                if (cards_str[i] != "")
                {
                    played_cards[i] = new Card(cards_str[i].Split('*')[0], cards_str[i].Split('*')[1]);
                    counter++;
                }

            Card selected = pack[0];
            // Algorithm
            string format = counter + "played_card:" + selected.GetCardType() + "*" + selected.GetCardRank();
            return format;
        }
        public static int GetOrder(int counter)
        {
            switch (counter)
            {
                //1 3 2 4
                case 0:
                    //im first
                    if (ID == 1)
                        return {1, 3, 2, 4};
                    if (ID == 2)
                        return {2, 4, 1, 3};
                    if (ID == 3)
                        return {3, 2, 4, 1};
                    if (ID == 4)
                        return {4, 1, 3, 2};
                case 1:
                    //im second
                    if (ID == 1)
                        return {4, 1, 3, 2};
                    if (ID == 2)
                        return {3, 2, 4, 1};
                    if (ID == 3)
                        return {1, 3, 2, 4};
                    if (ID == 4)
                        return {2, 4, 1, 3};
                case 2:
                    //im third
                    if (ID == 1)
                        return {2, 4, 1, 3};
                    if (ID == 2)
                        return {1, 3, 2, 4};
                    if (ID == 3)
                        return {4, 1, 3, 2};
                    if (ID == 4)
                        return {3, 2, 4, 1};
                case 3:
                    //im fourth
                    if (ID == 1)
                        return {3, 2, 4, 1};
                    if (ID == 2)
                        return {4, 1, 3, 2};
                    if (ID == 3)
                        return {2, 4, 1, 3};
                    if (ID == 4)
                        return {1, 3, 2, 4};

            }

        }
        public static void Discover(int counter, Card[] played_cards)
        {
            //updates the current biggest card in each suit
            foreach(Card card in played_cards)
            {
                if (card.GetCardType()=="SPADES" && card.GetValue() == big_card[0])
                    big_card[0]--;
                else if (card.GetCardType()=="CLUBS" && card.GetValue() == big_card[1])
                    big_card[1]--;
                else if (card.GetCardType()=="DIAMONDS" && card.GetValue() == big_card[2])
                    big_card[2]--;
                else if (card.GetCardType()=="HEARTS" && card.GetValue() == big_card[3])
                    big_card[3]--;
            }
            //1 3 2 4

            Card first_card = played_cards[GetOrder(counter)[0]];
            for (int i = 0; i < played_cards.Length; i++)
            {
                if (first_card.GetCardType()() != played_cards[i].GetCardType()() && first_card.GetCardType()()!=strong)
                    discover[i+1]= discover[i+1] + "KILL " + first_card.GetCardType()()+"|";
            }
            if (first_card.GetCardType()()==strong)
                for (int i=0; i<played_cards.Length;i++)
                    if (first_card.GetCardType()() != played_cards[i].GetCardType()())
                        discover[i + 1] = discover[i+1]+"NO STRONG|";
            int current_winner = GetCurrentWinner(played_cards,counter);
            Card winner_card = GetCurrentWinnerCard(played_cards,counter);
            int[] order = GetOrder(counter);
            int first_player = order[0];
            if (first_player!=partner_id)
            {
                Card partner_card = played_cards[partner_id-1];
                if (winner_card.GetCardType()==first_card.GetCardType())
                    if (partner_card.GetCardType()==winner_card.GetCardType())
                        if (winner_card.GetValue()-partner_card.GetValue()<3)
                            discover[partner_id] = discover[partner_id] + "KILL "+
                                partner_card.GetCardType()+"|";

            }

        }
        public static Card killSmall (int counter, Card[] played_cards)
        {
            int begginer = GetOrder(counter)[0];
            Card first_card = Card[begginer];
            bool have_type = false;
            for (int i=0; i<pack.Length; i++)
            {
                if (first_card.GetCardType() == pack[i].GetCardType())
                    have_type = true;
            }
            bool killed_enemy = false;
            Card killer = null;
            for (int i=0; i < played_cards.Length; i++)
            {
                if (first_card.GetCardType()!=played_cards[i].GetCardType() 
                    && played_cards[i].GetCardType() == strong)
                {
                    if (killer==null)
                        killer = played_cards[i];
                    else
                    {
                        if (killer.GetValue() < played_cards[i].GetValue())
                            killer = played_cards[i];
                    }
                    if (i+1==enemy1||i+1==enemy2)
                    {
                        killed_enemy = true;
                    }

                }
            }

            int strong_counter=0;
            Card my_card=new Card("DIAMONDS", "rank_A");
            if (!have_type && GetCurrentWinner()!=partner_id )
            {
                if (!killed_enemy)
                {
                    for (int i = 0; i < pack.Length; i++)
                    {
                        if (pack[i].GetCardType() == strong)
                            {
                                strong_counter++;
                                if (pack[i].GetValue() < my_card.GetValue())
                                    my_card = pack[i];
                            }
                    }
                }
                else
                {
                    for (int i = 0; i < pack.Length; i++)
                    {
                        if (pack[i].GetCardType() == strong)
                            {
                                strong_counter++;
                                if (pack[i].GetValue() > killer.GetValue())
                                    my_card = pack[i];
                            }
                    }
                }
            }
            if (strong_counter>4 && my_card.GetValue()<10)
                return my_card;
            else
            {
                Card minCard = new Card("DIAMONDS", "rank_A");
                for (int i=0; i<pack.Length; i++)
                    {
                        if (pack[i].GetValue()<minCard.GetValue())
                            minCard = pack[i];
                    }
                return minCard;
            }
        }
        //if im first
        public static Card IfPartnerKillsSomething()
        {
            string[] partner_discover = discover[partner_id].Split('|');
            string[] kills = new string[4];
            for (int i=0; i<partner_discover;i++)
                if (partner_discover[i].Contains("KILL"))
                    kills[i] = partner_discover[i].Split('KILL ')[1];
            
            Card my_card = new Card("DIAMONDS", "rank_A");
            for (int i=0; i<pack.Length; i++)
            {
                for (int j=0; j<kills.Length; j++)
                    if (pack[i].GetCardType()() == kills[j])
                        if (my_card.GetValue() > pack[i].GetValue())
                            my_card = pack[i];

            }
            if (my_card.GetCardRank()!="rank_A" && my_card.GetCardType()!="DIAMONDS")                
                return my_card;
            else
                //use other algorithem or put random card

        }
        public static int GetCurrentWinner(Card[] played_cards, int counter)
        {
            Card current_winner_card = played_cards[0];
            int current_winner_id=1;
            for (int i=1; i < played_cards.Length; i++)
            {
                if (current_winner_card.GetValue()<played_cards[i].GetValue() 
                    && current_winner_card.GetCardType() == played_cards[i].GetCardType())
                {
                    current_winner_card=played_cards[i]; 
                    current_winner_id=i+1;
                }
                else
                {
                    if (played_cards[i].GetCardType()==strong)
                    {
                        current_winner_card = played_cards[i];
                        current_winner_id=i+1;
                    }    
                }
                       
            }
            return current_winner_id;
        }
        public static Card GetCurrentWinnerCard(Card[] played_cards, int counter)
        {
            return played_cards[GetCurrentWinner(played_cards, counter)-1];
        }
       

    }

}
