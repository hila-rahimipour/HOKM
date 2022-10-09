using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace HOKM.Code
{
    public class Networking
    {

        public static Socket OpenSocket(string ip, int port)
        {
            IPAddress addr = IPAddress.Parse(ip);
            IPEndPoint server = new IPEndPoint(addr, port);
            Socket sock = new Socket(addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sock.Connect(server);
            return sock;
        }

        public static void SendMessage(Socket sock, string data)
        {
            string new_data = data.Length.ToString().PadLeft(8, '0') + data;
            byte[] mes = Encoding.UTF8.GetBytes(new_data);
            sock.Send(mes);
        }

        public static string RecvMessage(Socket sock)
        {
            byte[] mes = new byte[8];
            sock.Receive(mes);
            int len = int.Parse(Encoding.UTF8.GetString(mes));
            mes = new byte[len];
            sock.Receive(mes);
            return Encoding.UTF8.GetString(mes);
        }

        public static void CloseSocket(Socket sock)
        {
            sock.Shutdown(SocketShutdown.Both);
            sock.Close();
        }
    }
}
