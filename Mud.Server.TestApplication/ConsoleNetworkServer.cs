using System;
using System.Diagnostics;
using System.Threading;
using AutoMapper;
using Mud.Container;
using Mud.Network;
using Mud.Repository;

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

            // Initialize IOC container
            DependencyContainer.Current.Register<IWorld, World.World>(SimpleInjector.Lifestyle.Singleton);
            DependencyContainer.Current.Register<IServer, Server.Server>(SimpleInjector.Lifestyle.Singleton);
            DependencyContainer.Current.Register<ITimeHandler, Server.Server>(SimpleInjector.Lifestyle.Singleton); // Server also implements ITimeHandler
            DependencyContainer.Current.Register<IWiznet, Server.Server>(SimpleInjector.Lifestyle.Singleton); // Server also implements IWiznet
            DependencyContainer.Current.Register<IPlayerManager, Server.Server>(SimpleInjector.Lifestyle.Singleton); // Server also implements IPlayerManager
            DependencyContainer.Current.Register<IAdminManager, Server.Server>(SimpleInjector.Lifestyle.Singleton); // Server also implements IAdminManager
            DependencyContainer.Current.Register<IAbilityManager, Abilities.AbilityManager>(SimpleInjector.Lifestyle.Singleton);
            DependencyContainer.Current.Register<IClassManager, Classes.ClassManager>(SimpleInjector.Lifestyle.Singleton);
            DependencyContainer.Current.Register<IRaceManager, Races.RaceManager>(SimpleInjector.Lifestyle.Singleton);

            DependencyContainer.Current.Register<ILoginRepository, Repository.Filesystem.LoginRepository>(SimpleInjector.Lifestyle.Singleton);
            DependencyContainer.Current.Register<IPlayerRepository, Repository.Filesystem.PlayerRepository>(SimpleInjector.Lifestyle.Singleton);
            DependencyContainer.Current.Register<IAdminRepository, Repository.Filesystem.AdminRepository>(SimpleInjector.Lifestyle.Singleton);

            // Initialize mapping
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<Repository.Filesystem.AutoMapperProfile>();
                cfg.AddProfile<Repository.Mongo.AutoMapperProfile>();
            });
            DependencyContainer.Current.RegisterInstance(mapperConfiguration.CreateMapper());
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
                                foreach (IAdmin a in DependencyContainer.Current.GetInstance<IAdminManager>().Admins)
                                    Console.WriteLine(a.DisplayName + " " + a.PlayerState + " " + (a.Impersonating != null ? a.Impersonating.DisplayName : "") + " " + (a.Incarnating != null ? a.Incarnating.DisplayName : ""));
                            }
                            else if (line == "plist")
                            {
                                Console.WriteLine("players:");
                                foreach (IPlayer p in DependencyContainer.Current.GetInstance<IPlayerManager>().Players)
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
