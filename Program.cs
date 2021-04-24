using System;
using System.Net;
using System.Net.Sockets;

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

            Console.ReadKey();

        }

        private static void RequestCompleted(object sender, SocketAsyncEventArgs e)
        {
            Console.WriteLine("Something Connected!");
            while (e.AcceptSocket.Connected)
            {
                byte[] data = new byte[256];
                var uwu = e.AcceptSocket.Receive(data);
                Console.WriteLine(uwu);
                if (uwu != 0) {
                    
                    if (System.Text.Encoding.UTF8.GetString(data).Contains("disconnect")) {
                        e.AcceptSocket.Disconnect(true);
                        continue;
                    }
                    Console.WriteLine(System.Text.Encoding.UTF8.GetString(data));
                }
            }
            Console.WriteLine("Disconnected.");
        }
    }
}
