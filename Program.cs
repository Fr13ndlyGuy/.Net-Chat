using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Net_Chat_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress ip = IPAddress.Parse("127.0.0.1");
            IPEndPoint ep = new IPEndPoint(ip, 1337);

            socket.Bind(ep);

            socket.Listen(100);


            SocketAsyncEventArgs controls = new SocketAsyncEventArgs();
            controls.Completed += RequestCompleted;

            socket.AcceptAsync(controls);

            Console.WriteLine("Working.");

            while (true) {
                continue;
            }

        }

        private static void RequestCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                Console.WriteLine(string.Format("{0} connected", e.AcceptSocket.RemoteEndPoint.ToString()));
                Chat.AddUser(new UserData(e.AcceptSocket));
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(string.Format("Error: {0} at {1}", ex.Message, ex.StackTrace));

                Console.ReadKey();
            }
        }
    }
}
