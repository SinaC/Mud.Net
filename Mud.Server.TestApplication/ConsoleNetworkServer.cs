using System;
using System.Diagnostics;
using System.Threading;
using Mud.Network;

namespace Mud.Server.TestApplication
{
    public class ConsoleNetworkServer : INetworkServer
    {
        public event NewClientConnectedEventHandler NewClientConnected;

        private ConsoleClient _client;
        private bool _stopped;

        public IClient AddClient(string name, bool displayPlayerName, bool colorAccepted)
        {
            Debug.Assert(_client == null, "Client already added");
            ConsoleClient client = new ConsoleClient(name)
            {
                DisplayPlayerName = displayPlayerName,
                ColorAccepted = colorAccepted
            };
            _client = client;
            if (NewClientConnected != null)
                NewClientConnected(client);
            return client;
        }

        public void Initialize()
        {
            _stopped = false;
        }

        public void Start()
        {
            while (!_stopped)
            {
                if (Console.KeyAvailable)
                {
                    string line;
                    bool isEchoOff = _client != null && _client.IsEchoOff;
                    if (isEchoOff)
                    {
                        line = String.Empty;
                        while (true) // http://stackoverflow.com/questions/3404421/password-masking-console-application
                        {
                            ConsoleKeyInfo key = Console.ReadKey(true);
                            if (key.Key == ConsoleKey.Enter)
                            {
                                Console.WriteLine();
                                break;
                            }
                            if (key.Key != ConsoleKey.Backspace)
                            {
                                Console.Write("*");
                                line += key.KeyChar;
                            }
                            else if (line.Length > 0)
                            {
                                line = line.Substring(0, line.Length - 1);
                                Console.Write("\b \b");
                            }
                        }
                    }
                    else
                        line = Console.ReadLine();
                    if (line != null)
                    {
                        // server commands
                        if (line.StartsWith("#"))
                        {
                            line = line.Replace("#", String.Empty).ToLower();
                            if (line == "exit" || line == "quit")
                            {
                                _stopped = true;
                                break;
                            }
                            else if (line == "alist")
                            {
                                Console.WriteLine("Admins:");
                                foreach (IAdmin a in Server.Server.Instance.GetAdmins())
                                    Console.WriteLine(a.Name + " " + a.PlayerState + " " + (a.Impersonating != null ? a.Impersonating.Name : "") + " " + (a.Incarnating != null ? a.Incarnating.Name : ""));
                            }
                            else if (line == "plist")
                            {
                                Console.WriteLine("players:");
                                foreach (IPlayer p in Server.Server.Instance.GetPlayers())
                                    Console.WriteLine(p.Name + " " + p.PlayerState + " " + (p.Impersonating != null ? p.Impersonating.Name : ""));
                            }
                            // TODO: characters/rooms/items
                        }
                        // client commands
                        else
                            _client.OnDataReceived(line);
                    }
                }
                else
                    Thread.Sleep(100);
            }
        }

        public void Stop()
        {
            _stopped = true;
        }
    }
}
