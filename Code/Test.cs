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

        private static string SERVER_ADDR = "192.168.0.1";
        private static int SERVER_PORT = 55555;
        private static int ID = -1;
        private static string USERNAME = "Name";

        public static void Main(string[] args)
        {

            IPAddress ipAddr = IPAddress.Parse(SERVER_ADDR);
            IPEndPoint server = new IPEndPoint(ipAddr, SERVER_PORT);
            Socket sock = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sock.Connect(server);

            //byte[] messageSent = Encoding.ASCII.GetBytes("Test Client<EOF>");
            //sock.Send(messageSent);

            // Receive Client ID:
            byte[] id_mes = new byte[1024];
            sock.Receive(id_mes);
            string id_text = Encoding.ASCII.GetString(id_mes);
            ID = int.Parse(id_text.Substring(id_text.Length - 1));
            Console.WriteLine("This client's ID is " + ID);

            // Send Username:
            byte[] name_mes = Encoding.ASCII.GetBytes("username:" + USERNAME);
            sock.Send(name_mes);

            sock.Shutdown(SocketShutdown.Both);
            sock.Close();
        }

        
    }

}
