using System;
using System.Collections.Generic;
using System.Linq;

using System.Net.Sockets;

using System.Text;
using System.Text.RegularExpressions;

using System.Threading;
using System.Threading.Tasks;

namespace Net_Chat_Server
{

    static class Chat
    {
        static List<UserData> Users = new List<UserData>();

        public static void AddUser(UserData user)
        {
            Thread newRead = new Thread(user.Receive);
            newRead.Start();
            user.Send(Encoding.UTF8.GetBytes("Welcome to .Net Chat!"));
            user.OnMessageReceived += SendMessage;
            Users.Add(user);
        }

        public static void Remove(UserData user)
        {
            Users.Remove(user);
        }

        static void SendMessage(UserData sender, string message) {
            message = message.Replace(Encoding.UTF8.GetString(new byte[] { 0 }), "").TrimEnd();
            if (message.StartsWith("/")) {
                message = message.Replace("/", "");
                var split = message.Split(" ");
                var cmd = split[0];
                var args = "";
                for (int i = 1; i < split.Length; i++)
                {
                    args += split[i];
                }
                split = null;

                switch (cmd)
                {
                    default:
                        sender.Send(Encoding.UTF8.GetBytes("Unknown command."));
                        break;
                    case "nick":
                        sender.Name = args;
                        break;
                }
                return;
            }

            foreach (UserData receiver in Users)
            {
                receiver.Send(Encoding.UTF8.GetBytes(message));
            }
        }
    }

    static class Censorship
    {
        static string NameMask = "(G|g).*(A|a|4|/\\|А|а).*(Y|y).*;(N|n).*(G|g).*(E|e)*.*"; // Masks for g-word and n-word splitted by ;
        public static bool CheckUsername(string name)
        {
            bool matched = false;
            foreach (string mask in NameMask.Split(';'))
            {
                if (matched) break;
                matched = Regex.IsMatch(name, mask);
            }
            return !matched;
        }
    }

    class UserData {

        string _name = "Anonymous";

        public string Name {
            get {
                return _name;
            }
            set {
                _name = Censorship.CheckUsername(value) ? value : "Censored";
            }
        }

        public delegate void OnMessage(UserData sender, string message);

        public event OnMessage OnMessageReceived;


        Dictionary<string, Socket> Sockets = new Dictionary<string, Socket>() {
            ["Read"] = null,
            ["Write"] = null
        };

        public void Send(byte[] data)
        {
            if (Sockets["Write"].Connected)
            {
                Sockets["Write"].Send(data);
            }
        }

        public void Send(string data)
        {
            if (Sockets["Write"].Connected)
            {
                Sockets["Write"].Send(Encoding.UTF8.GetBytes(data));
            }
        }


        public void Receive()
        {
            while (Sockets["Read"].Connected) {
                byte[] Received = new byte[256];
                if (Sockets["Read"].Receive(Received) != 0)
                {
                    var message = string.Format("{0}: {1}", Name, Encoding.UTF8.GetString(Received));
                    if (Encoding.UTF8.GetString(Received).StartsWith("MSG:"))
                    {
                        message = message.Replace("MSG:", "");
                    }
                    if (Encoding.UTF8.GetString(Received).StartsWith("/"))
                    {
                        message = string.Format("{0}", Encoding.UTF8.GetString(Received));
                    }
                    
                    Console.WriteLine(message);
                    OnMessageReceived(this, message);
                }
            }
            Chat.Remove(this);
            Console.WriteLine("Disconnected");
        }


        public UserData(Socket socket)
        {
            Sockets["Read"] = socket;
            Sockets["Write"] = socket;
        }

        public UserData(Socket receive, Socket send)
        {
            Sockets["Read"] = receive;
            Sockets["Write"] = send;
        }

    }
}
