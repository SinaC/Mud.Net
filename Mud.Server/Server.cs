using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mud.Logger;
using Mud.Network;

namespace Mud.Server
{
    public class Server : IServer
    {
        private const int PulsePerSeconds = 8;
        private const int PulseDelay = 1000/PulsePerSeconds;

        private class PlayerClient
        {
            public IClient Client { get; set; }
            public IPlayer Player { get; set; }

            private readonly ConcurrentQueue<string> _receiveQueue;
            //private readonly ConcurrentQueue<string> _sendQueue; // when using this, ProcessOutput must loop until DataToSend returns null
            private readonly StringBuilder _sendBuffer;

            public PlayerClient()
            {
                _receiveQueue = new ConcurrentQueue<string>();
                _sendBuffer = new StringBuilder();
            }

            // Used in synchronous mode
            public void EnqueueReceivedData(string data)
            {
                _receiveQueue.Enqueue(data);
            }

            public string DequeueReceivedData()
            {
                string data;
                bool dequeued = _receiveQueue.TryDequeue(out data);
                return dequeued ? data : null;
            }

            public void EnqueueDataToSend(string data)
            {
                //_sendQueue.Enqueue(message);
                lock (_sendBuffer)
                    _sendBuffer.Append(data);
            }

            public string DequeueDataToSend()
            {
                //string data;
                //bool taken = _sendQueue.TryDequeue(out data);
                //return taken ? data : null;
                lock (_sendBuffer)
                {
                    string data = _sendBuffer.ToString();
                    _sendBuffer.Clear();
                    return data;
                }
            }
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

        public bool IsAsynchronous { get; private set; }

        public void Initialize(bool asynchronous, INetworkServer networkServer)
        {
            IsAsynchronous = asynchronous;
            _networkServer = networkServer;
            if (networkServer != null) // TODO: remove  network server must be mandatory
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
            int minutes = seconds/60;
            int remaining = seconds%60;
            if (minutes > 0 && remaining != 0)
                Broadcast(String.Format("%R%Shutdown in {0} minute{1} and {2} second{3}%x%", minutes, minutes > 1 ? "s" : String.Empty, remaining, remaining > 1 ? "s" : String.Empty));
            else if (minutes > 0 && remaining == 0)
                Broadcast(String.Format("%R%Shutdown in {0} minute{1}%x%", minutes, minutes > 1 ? "s" : String.Empty));
            else
                Broadcast(String.Format("%R%Shutdown in {0} second{1}%x%", seconds, seconds > 1 ? "s" : String.Empty));
            _pulseBeforeShutdown = seconds*PulsePerSeconds;
        }

        // TODO: remove
        // TEST PURPOSE
        public IPlayer AddPlayer(IClient client, string name)
        {
            IPlayer player = new Player.Player(Guid.NewGuid(), name);
            player.SendData += PlayerOnSendData;
            client.DataReceived += ClientOnDataReceived;
            client.Disconnected += ClientOnDisconnected;
            PlayerClient playerClient = new PlayerClient
            {
                Client = client,
                Player = player
            };
            _playerClients.Add(playerClient);
            World.World.Instance.AddPlayer(player);

            player.Send("Welcome {0}", name);

            return player;
        }

        public IAdmin AddAdmin(IClient client, string name)
        {
            IAdmin admin = new Admin.Admin(Guid.NewGuid(), name);
            admin.SendData += PlayerOnSendData;
            client.DataReceived += ClientOnDataReceived;
            client.Disconnected += ClientOnDisconnected;
            PlayerClient playerClient = new PlayerClient
            {
                Client = client,
                Player = admin
            };
            _playerClients.Add(playerClient);
            World.World.Instance.AddAdmin(admin);

            admin.Send("Welcome master {0}", name);

            return admin;
        }

        #region Event handlers

        private void NetworkServerOnNewClientConnected(IClient client)
        {
            // TODO: how can we determine if admin or player ?
            IPlayer player = new Player.Player(Guid.NewGuid());
            client.DataReceived += ClientOnDataReceived;
            client.Disconnected += ClientOnDisconnected;
            player.SendData += PlayerOnSendData;
            PlayerClient playerClient = new PlayerClient
            {
                Client = client,
                Player = player
            };
            _playerClients.Add(playerClient);

            player.Send("Why don't you login or tell us the name you wish to be known by?");
        }

        private void ClientOnDisconnected(IClient client)
        {
            PlayerClient playerClient = _playerClients.FirstOrDefault(x => x.Client == client);
            if (playerClient == null)
                Log.Default.WriteLine(LogLevels.Error, "ClientOnDisconnected: null client");
            else
            {
                playerClient.Player.OnDisconnected();
                client.DataReceived -= ClientOnDataReceived;
                client.Disconnected -= ClientOnDisconnected;
                playerClient.Player.SendData -= PlayerOnSendData;
                _playerClients.Remove(playerClient);
            }
        }

        private void ClientOnDataReceived(IClient client, string data)
        {
            PlayerClient playerClient = _playerClients.FirstOrDefault(x => x.Client == client);
            if (playerClient == null)
                Log.Default.WriteLine(LogLevels.Error, "ClientOnDataReceived: null client");
            else
            {
                if (IsAsynchronous)
                    playerClient.Player.ProcessCommand(data);
                else
                    playerClient.EnqueueReceivedData(data);
            }
        }

        private void PlayerOnSendData(IPlayer player, string data)
        {
            PlayerClient playerClient = _playerClients.FirstOrDefault(x => x.Player == player);
            if (playerClient == null)
                Log.Default.WriteLine(LogLevels.Error, "PlayerOnSendData: null client");
            else
            {
                if (IsAsynchronous)
                    playerClient.Client.WriteData(data);
                else
                    playerClient.EnqueueDataToSend(data);
            }
        }

        #endregion

        private void Broadcast(string message)
        {
            message = message + Environment.NewLine;
            // By-pass asynchronous/synchronous send
            foreach (PlayerClient playerClient in _playerClients)
                playerClient.Client.WriteData(message);
        }

        private void ProcessInput()
        {
            // Read one command from each client and process it
            if (!IsAsynchronous)
            {
                foreach (PlayerClient playerClient in _playerClients) // TODO: first connected player will be processed before other, try a randomize
                {
                    string data = playerClient.DequeueReceivedData(); // process one command at a time
                    if (!String.IsNullOrWhiteSpace(data))
                        playerClient.Player.ProcessCommand(data);
                }
            }
        }

        private void ProcessOutput()
        {
            if (!IsAsynchronous)
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
                    string data = playerClient.DequeueDataToSend();
                    if (!String.IsNullOrWhiteSpace(data))
                        playerClient.Client.WriteData(data);
                }
            }
        }

        private void PulseShutdown()
        {
            if (_pulseBeforeShutdown >= 0)
            {
                _pulseBeforeShutdown--;
                if (_pulseBeforeShutdown == PulsePerSeconds * 60 * 15)
                    Broadcast("%R%Shutdown in 15 minutes%x%");
                if (_pulseBeforeShutdown == PulsePerSeconds * 60 * 10)
                    Broadcast("%R%Shutdown in 10 minutes%x%");
                if (_pulseBeforeShutdown == PulsePerSeconds * 60 * 5)
                    Broadcast("%R%Shutdown in 5 minutes%x%");
                if (_pulseBeforeShutdown == PulsePerSeconds * 60)
                    Broadcast("%R%Shutdown in 1 minute%x%");
                if (_pulseBeforeShutdown == PulsePerSeconds * 30)
                    Broadcast("%R%Shutdown in 30 seconds%x%");
                if (_pulseBeforeShutdown == PulsePerSeconds * 15)
                    Broadcast("%R%Shutdown in 15 seconds%x%");
                if (_pulseBeforeShutdown == PulsePerSeconds * 10)
                    Broadcast("%R%Shutdown in 10 seconds%x%");
                if (_pulseBeforeShutdown == PulsePerSeconds * 5)
                    Broadcast("%R%Shutdown in 5%x%");
                if (_pulseBeforeShutdown == PulsePerSeconds * 4)
                    Broadcast("%R%Shutdown in 4%x%");
                if (_pulseBeforeShutdown == PulsePerSeconds * 3)
                    Broadcast("%R%Shutdown in 3%x%");
                if (_pulseBeforeShutdown == PulsePerSeconds * 2)
                    Broadcast("%R%Shutdown in 2%x%");
                if (_pulseBeforeShutdown == PulsePerSeconds * 1)
                    Broadcast("%R%Shutdown in 1%x%");
                if (_pulseBeforeShutdown == 0)
                {
                    Broadcast("%R%Shutdown NOW!!!%x%");
                    Stop();
                }
            }
        }

        private void DoPulse()
        {
            // TODO:  (see handler.c)
            //Log.Default.WriteLine(LogLevels.Debug, "PULSE: {0:HH:mm:ss.ffffff}", DateTime.Now);

            PulseShutdown();
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
                        int sleepTime = PulseDelay - (int) elapsedMs;
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
