using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Mud.Logger;
using Mud.Network;

namespace Mud.Server
{
    public class Server : IServer
    {
        private const int PulsePerSeconds = 4;
        private const int PulseDelay = 1000 / PulsePerSeconds;

        private class PlayerClient
        {
            public IClient Client { get; set; }
            public IPlayer Player { get; set; }
        }

        private readonly List<PlayerClient> _playerClients;

        private INetworkServer _networkServer;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _gameLoopTask;

        private volatile int _pulseBeforeShutdown; // pulse count before shutdown

        #region Singleton

        private static readonly Lazy<Server> Lazy = new Lazy<Server>(() => new Server());

        public static IServer Instance
        {
            get { return Lazy.Value; }
        }

        private Server()
        {
            _playerClients = new List<PlayerClient>();
        }

        #endregion

        public void Initialize(INetworkServer networkServer)
        {
            _networkServer = networkServer;
            _networkServer.NewClientConnected += NetworkServerOnNewClientConnected;
        }

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _gameLoopTask = Task.Factory.StartNew(GameLoopTask, _cancellationTokenSource.Token);

            if (_networkServer != null)
            {
                _networkServer.Initialize();
                _networkServer.Start();
            }

        }

        public void Stop()
        {
            try
            {
                if (_networkServer != null)
                    _networkServer.Stop();

                _cancellationTokenSource.Cancel();
                _gameLoopTask.Wait(2000, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException ex)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Operation canceled exception while stopping. Exception: {0}", ex);
            }
            catch (AggregateException ex)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Aggregate exception while stopping. Exception: {0}", ex.Flatten());
            }
        }

        public void Shutdown(int seconds)
        {
            _pulseBeforeShutdown = seconds*PulsePerSeconds;
        }

        // TODO: remove
        // TEST PURPOSE
        public IPlayer AddClient(IClient client, string name)
        {
            IPlayer player = new Player.Player(client, Guid.NewGuid(), name);
            PlayerClient playerClient = new PlayerClient
            {
                Client = client,
                Player = player
            };
            _playerClients.Add(playerClient);
            World.World.Instance.AddPlayer(player);

            player.Send("Let's go");

            return player;
        }

        public IAdmin AddAdmin(IClient client, string name)
        {
            IAdmin admin = new Admin.Admin(client, Guid.NewGuid(), name);
            PlayerClient playerClient = new PlayerClient
            {
                Client = client,
                Player = admin
            };
            _playerClients.Add(playerClient);
            World.World.Instance.AddAdmin(admin);

            admin.Send("Let's go");
            
            return admin;
        }

        #region INetworkServer events handler
        
        private void NetworkServerOnNewClientConnected(IClient client)
        {
            IPlayer player = new Player.Player(client, Guid.NewGuid());
            PlayerClient playerClient = new PlayerClient
            {
                Client = client,
                Player = player
            };
            _playerClients.Add(playerClient);

            player.Send("Why don't you login or tell us the name you wish to be known by?");
        }

        #endregion

        private void Broadcast(string message)
        {
            // By-pass asynchronous/synchronous send
            foreach (PlayerClient playerClient in _playerClients)
                playerClient.Client.WriteData(message);
        }

        private void ProcessInput()
        {
            // Read one command from each client and process it
            if (!ServerOptions.AsynchronousReceive)
            {
                foreach (PlayerClient playerClient in _playerClients) // TODO: first connected player will be processed before other, try a randomize
                {
                    string data = playerClient.Client.ReadData(); // process one command at a time
                    if (!String.IsNullOrWhiteSpace(data))
                        playerClient.Player.ProcessCommand(data);
                }
            }
        }

        private void ProcessOutput()
        {
            if (!ServerOptions.AsynchronousSend)
            {
                foreach (PlayerClient playerClient in _playerClients)
                {
                    // This code must be uncommented if Queue is used in Player
                    //while (true) // process all current output for one player
                    //{
                    //    string data = playerClient.Player.DataToSend();
                    //    if (!String.IsNullOrWhiteSpace(data))
                    //        playerClient.Client.WriteData(data);
                    //    else
                    //        break;
                    //}
                    string data = playerClient.Player.DataToSend();
                    if (!String.IsNullOrWhiteSpace(data))
                        playerClient.Client.WriteData(data);
                }
            }
        }

        private void DoPulse()
        {
            // TODO:  (see handler.c)
            //Log.Default.WriteLine(LogLevels.Debug, "PULSE: {0:HH:mm:ss.ffffff}", DateTime.Now);

            if (_pulseBeforeShutdown >= 0)
            {
                _pulseBeforeShutdown--;
                if (_pulseBeforeShutdown == PulsePerSeconds * 5)
                    Broadcast("Shutdown in 5");
                if (_pulseBeforeShutdown == PulsePerSeconds * 4)
                    Broadcast("Shutdown in 4");
                if (_pulseBeforeShutdown == PulsePerSeconds * 3)
                    Broadcast("Shutdown in 3");
                if (_pulseBeforeShutdown == PulsePerSeconds * 2)
                    Broadcast("Shutdown in 2");
                if (_pulseBeforeShutdown == PulsePerSeconds * 1)
                    Broadcast("Shutdown in 1");
                if (_pulseBeforeShutdown == 0)
                {
                    Broadcast("Shutdown NOW!!!");
                    Stop();
                }
            }
        }

        private void GameLoopTask()
        {
            // TODO:  (see comm.C:881)
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                while (true)
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        Log.Default.WriteLine(LogLevels.Info, "Stop GameLoopTask requested");
                        break;
                    }

                    ProcessInput();

                    DoPulse();

                    ProcessOutput();

                    sw.Stop();
                    long elapsedMs = sw.ElapsedMilliseconds; // in milliseconds
                    if (elapsedMs < PulseDelay)
                    {
                        //long elapsedTick = sw.ElapsedTicks; // 1 tick = 1 second/Stopwatch.Frequency
                        //long elapsedNs = sw.Elapsed.Ticks; // 1 tick = 1 nanosecond
                        //Log.Default.WriteLine(LogLevels.Debug, "Elapsed {0}Ms {1}Ticks {2}Ns", elapsedMs, elapsedTick, elapsedNs);
                        //Thread.Sleep(250 - (int) elapsedMs);
                        int sleepTime = PulseDelay - (int)elapsedMs;
                        _cancellationTokenSource.Token.WaitHandle.WaitOne(sleepTime);
                    }
                    else
                    {
                        Log.Default.WriteLine(LogLevels.Error, "!!! No sleep for GameLoopTask. Elapsed {0}", elapsedMs);
                        _cancellationTokenSource.Token.WaitHandle.WaitOne(1);
                    }
                    sw.Restart();
                }
            }
            catch (TaskCanceledException ex)
            {
                Log.Default.WriteLine(LogLevels.Error, "GameLoopTask exception. Exception: {0}", ex);
            }

            Log.Default.WriteLine(LogLevels.Info, "GameLoopTask stopped");
        }
    }
}
