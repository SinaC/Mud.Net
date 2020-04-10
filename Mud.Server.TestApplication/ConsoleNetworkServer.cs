using System;
using System.Diagnostics;
using System.Threading;
using Mud.Network;

namespace Mud.Server.TestApplication
{
    public class ConsoleNetworkServer : INetworkServer
    {
        public event NewClientConnectedEventHandler NewClientConnected;
        public event ClientDisconnectedEventHandler ClientDisconnected;

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
            NewClientConnected?.Invoke(client);
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
                        line = string.Empty;
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
                            line = line.Replace("#", string.Empty).ToLower();
                            if (line == "quit")
                            {
                                _stopped = true;
                                ClientDisconnected?.Invoke(_client);
                                break;
                            }
                            else if (line == "alist")
                            {
                                Console.WriteLine("Admins:");
                                foreach (IAdmin a in Repository.Server.Admins)
                                    Console.WriteLine(a.DisplayName + " " + a.PlayerState + " " + (a.Impersonating != null ? a.Impersonating.DisplayName : "") + " " + (a.Incarnating != null ? a.Incarnating.DisplayName : ""));
                            }
                            else if (line == "plist")
                            {
                                Console.WriteLine("players:");
                                foreach (IPlayer p in Repository.Server.Players)
                                    Console.WriteLine(p.DisplayName + " " + p.PlayerState + " " + (p.Impersonating != null ? p.Impersonating.DisplayName : ""));
                            }
                            // TODO: characters/rooms/items
                        }
                        // client commands
                        else
                            _client?.OnDataReceived(line);
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
