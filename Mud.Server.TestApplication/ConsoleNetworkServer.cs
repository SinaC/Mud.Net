using System;
using System.Threading.Tasks;
using Mud.Network;

namespace Mud.Server.TestApplication
{
    public class ConsoleNetworkServer : INetworkServer
    {
        public event NewClientConnectedEventHandler NewClientConnected;

        private ConsoleClient _client;
        private Task _inputTask;
        private bool _stopped;

        public IClient AddClient(string name, bool displayPlayerName, bool colorAccepted)
        {
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
            // NOP
            _stopped = false;
        }

        public void Start()
        {
            while (!_stopped)
            {
                if (Console.KeyAvailable)
                {
                    string line = Console.ReadLine();
                    if (!String.IsNullOrWhiteSpace(line))
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
                                foreach (IAdmin a in World.World.Instance.GetAdmins())
                                    Console.WriteLine(a.Name + " " + a.PlayerState + " " + (a.Impersonating != null ? a.Impersonating.Name : "") + " " + (a.Incarnating != null ? a.Incarnating.Name : ""));
                            }
                            else if (line == "plist")
                            {
                                Console.WriteLine("players:");
                                foreach (IPlayer p in World.World.Instance.GetPlayers())
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
                    System.Threading.Thread.Sleep(100);
            }
        }

        public void Stop()
        {
            _stopped = true;
        }
    }
}
