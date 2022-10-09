using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

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

        public static void Main(string[] args)
        {

            IPAddress ipAddr = IPAddress.Parse(SERVER_ADDR);
            IPEndPoint server = new IPEndPoint(ipAddr, SERVER_PORT);
            Socket sock = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sock.Connect(server);

            // Receive Client ID:
            byte[] id_mes = new byte[1024];
            sock.Receive(id_mes);
            string id_text = Encoding.ASCII.GetString(id_mes);
            ID = int.Parse(id_text.Split(':')[1]);
            Console.WriteLine("This client's ID is " + ID);

            // Send Username:
            //byte[] name_mes = Encoding.ASCII.GetBytes("username:" + USERNAME);
            //sock.Send(name_mes);

            // Determine Strong:
            strong = GetStrong(sock);
            if (strong != null)
            {
                while (true)
                {
                    sock.Send(Encoding.ASCII.GetBytes("set_strong:" + strong));
                    byte[] response = new byte[1024];
                    sock.Receive(response);
                    if (Encoding.ASCII.GetString(id_mes) == "ok")
                        break;
                }
            }

            BuildPack(sock);

            bool isGame = true;
            string result;

            // Play:
            while (isGame)
            {
                result = DoTurn(sock);
                if (result == "GAME_OVER")
                    isGame = false;
                else
                {
                    while (true)
                    {
                        sock.Send(Encoding.ASCII.GetBytes(result));
                        byte[] response = new byte[1024];
                        sock.Receive(response);
                        if (Encoding.ASCII.GetString(id_mes) == "ok")
                            break;
                        else if (Encoding.ASCII.GetString(id_mes) == "bad_play")
                        {
                            result = DoTurn(sock); //do something to change the selected card
                        }
                    }
                }
            }

            sock.Shutdown(SocketShutdown.Both);
            sock.Close();
        }

        public static string GetStrong(Socket sock)
        {
            byte[] mes = new byte[1024];
            sock.Receive(mes);
            string str_mes = Encoding.ASCII.GetString(mes);
            int ruler_id = int.Parse(str_mes.Split(':')[1]);

            mes = new byte[1024];
            sock.Receive(mes);

            if (ruler_id == ID)
            {
                Console.WriteLine("I am the ruler");
                string five_mes = Encoding.ASCII.GetString(mes);
                string[] five_string_arr = five_mes.Split('|');
                Card[] first_five = new Card[5];
                for (int i = 0; i < 5; i++)
                    first_five[i] = new Card(five_string_arr[i].Split('*')[0], five_string_arr[i].Split('*')[1]);

                string strong = "HEARTS";  // Placeholder
                // Algorithm
                return strong;
            }

            return null;
        }

        public static void BuildPack(Socket sock)
        {
            byte[] mes = new byte[1024];
            sock.Receive(mes);
            string str_mes = Encoding.ASCII.GetString(mes);
            str_mes = str_mes.Substring(9);

            string[] data = str_mes.Split(',');
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
            byte[] mes = new byte[1024];
            sock.Receive(mes);
            string str_mes = Encoding.ASCII.GetString(mes);
            str_mes = str_mes.Substring(9);

            if (str_mes == "GAME_OVER")
                return "GAME_OVER";

            string[] data = str_mes.Split(',');
            string suit = data[0].Split(':')[1];
            string[] cards_str = data[1].Split(':')[1].Split('|');
            Card[] played_cards = new Card[cards_str.Length];
            for (int i = 0; i < cards_str.Length; i++)
                if (cards_str[i] != "")
                {
                    Card c = new Card(cards_str[i].Split('*')[0], cards_str[i].Split('*')[1]);
                    played_cards[i] = c;
                    card_history.Add(c);
                }

            Card selected = pack[0];
            // Algorithm
            string format = "played_card:" + selected.GetCardType() + "*" + selected.GetCardRank();
            return format;
        }
    }

}
