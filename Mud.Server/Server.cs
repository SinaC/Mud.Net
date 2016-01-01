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
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server
{
    public class Server : IServer
    {
        private const int PulsePerSeconds = 8;
        private const int PulseDelay = 1000/PulsePerSeconds;

        private class Paging
        {
            private string[] _lines;
            private int _currentLine;

            public Paging()
            {
                _lines = null;
                _currentLine = 0;
            }

            public bool HasPageLeft
            {
                get { return _lines != null && _currentLine < _lines.Length; }
            }

            public void Clear()
            {
                _currentLine = 0;
                _lines = null;
            }

            public void SetData(StringBuilder data)
            {
                _currentLine = 0;
                _lines = data.ToString().Split(new[] {Environment.NewLine}, StringSplitOptions.None);
            }

            public string GetNextPage(int lineCount)
            {
                string lines = String.Join(Environment.NewLine, _lines.Skip(_currentLine).TakeWhile((n, i) => i < lineCount && i < _lines.Length)) + Environment.NewLine;
                _currentLine = Math.Min(_currentLine + lineCount, _lines.Length);
                return lines;
            }

            public string GetRemaining()
            {
                string lines = String.Join(Environment.NewLine, _lines);
                Clear();
                return lines;
            }
        }

        private class PlayerClient
        {
            public IClient Client { get; set; }
            public IPlayer Player { get; set; }

            private readonly ConcurrentQueue<string> _receiveQueue;
            //private readonly ConcurrentQueue<string> _sendQueue; // when using this, ProcessOutput must loop until DataToSend returns null
            private readonly StringBuilder _sendBuffer;

            private readonly Paging _paging;

            public Paging Paging
            {
                get { return _paging; }
            }

            public PlayerClient()
            {
                _receiveQueue = new ConcurrentQueue<string>();
                _sendBuffer = new StringBuilder();
                _paging = new Paging();
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

        // This allows fast lookup with client or player BUT both structures must be modified at the same time
        private readonly ConcurrentDictionary<IClient, PlayerClient> _clients;
        private readonly ConcurrentDictionary<IPlayer, PlayerClient> _players;
        //private readonly List<PlayerClient> _playerClients;

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
            //_playerClients = new List<PlayerClient>();
            _clients = new ConcurrentDictionary<IClient, PlayerClient>();
            _players = new ConcurrentDictionary<IPlayer, PlayerClient>();
        }

        #endregion

        #region IServer

        public bool IsAsynchronous { get; private set; }

        public void Initialize(INetworkServer networkServer, bool asynchronous)
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

        public IPlayer GetPlayer(CommandParameter parameter, bool perfectMatch)
        {
            //return FindHelpers.FindByName(_playerClients.Select(x => x.Player), parameter, perfectMatch);
            return FindHelpers.FindByName(_players.Keys, parameter, perfectMatch);
        }

        public IReadOnlyCollection<IPlayer> GetPlayers()
        {
            //return _playerClients.Select(x => x.Player).ToList().AsReadOnly();
            return _players.Keys.ToList().AsReadOnly();
        }

        public IAdmin GetAdmin(CommandParameter parameter, bool perfectMatch)
        {
            return null; // TODO
        }

        public IReadOnlyCollection<IAdmin> GetAdmins()
        {
            return null; // TODO
        }

        // TODO: remove
        // TEST PURPOSE
        public IPlayer AddPlayer(IClient client, string name)
        {
            IPlayer player = new Player.Player(Guid.NewGuid(), name);
            player.SendData += PlayerOnSendData;
            player.PageData += PlayerOnPageData;
            client.DataReceived += ClientOnDataReceived;
            client.Disconnected += ClientOnDisconnected;
            PlayerClient playerClient = new PlayerClient
            {
                Client = client,
                Player = player
            };
            //_playerClients.Add(playerClient);
            _players.AddOrUpdate(player, playerClient, (player1, client1) => client1); // TODO: updateValueFactory
            _clients.AddOrUpdate(client, playerClient, (player1, client1) => client1); // TODO: updateValueFactory

            player.Send("Welcome {0}" + Environment.NewLine, name);

            return player;
        }

        public IAdmin AddAdmin(IClient client, string name)
        {
            IAdmin admin = new Admin.Admin(Guid.NewGuid(), name);
            admin.SendData += PlayerOnSendData;
            admin.PageData += PlayerOnPageData;
            client.DataReceived += ClientOnDataReceived;
            client.Disconnected += ClientOnDisconnected;
            PlayerClient playerClient = new PlayerClient
            {
                Client = client,
                Player = admin
            };
            //_playerClients.Add(playerClient);
            _players.AddOrUpdate(admin, playerClient, (player1, client1) => client1); // TODO: updateValueFactory
            _clients.AddOrUpdate(client, playerClient, (player1, client1) => client1); // TODO: updateValueFactory

            admin.Send("Welcome master {0}" + Environment.NewLine, name);

            return admin;
        }

        #endregion

        #region Event handlers

        private void NetworkServerOnNewClientConnected(IClient client)
        {
            // TODO: how can we determine if admin or player ?
            IPlayer player = new Player.Player(Guid.NewGuid());
            client.DataReceived += ClientOnDataReceived;
            client.Disconnected += ClientOnDisconnected;
            player.SendData += PlayerOnSendData;
            player.PageData += PlayerOnPageData;
            PlayerClient playerClient = new PlayerClient
            {
                Client = client,
                Player = player
            };
            //_playerClients.Add(playerClient);
            // TODO: lock
            _players.AddOrUpdate(player, playerClient, (player1, client1) => client1); // TODO: updateValueFactory
            _clients.AddOrUpdate(client, playerClient, (player1, client1) => client1); // TODO: updateValueFactory

            player.Send("Why don't you login or tell us the name you wish to be known by?");
        }

        private void ClientOnDisconnected(IClient client)
        {
            //PlayerClient playerClient = _playerClients.FirstOrDefault(x => x.Client == client);
            // TOOD: lock
            PlayerClient playerClient;
            bool removed = _clients.TryRemove(client, out playerClient);

            //if (playerClient == null)
            if (removed)
                Log.Default.WriteLine(LogLevels.Error, "ClientOnDisconnected: null client");
            else
            {
                playerClient.Player.OnDisconnected();
                client.DataReceived -= ClientOnDataReceived;
                client.Disconnected -= ClientOnDisconnected;
                playerClient.Player.SendData -= PlayerOnSendData;
                playerClient.Player.PageData -= PlayerOnPageData;
                //_playerClients.Remove(playerClient);
                _players.TryRemove(playerClient.Player, out playerClient);
            }
        }

        private void ClientOnDataReceived(IClient client, string command)
        {
            PlayerClient playerClient;
            bool found = _clients.TryGetValue(client, out playerClient);
            if (playerClient == null)
                Log.Default.WriteLine(LogLevels.Error, "ClientOnDataReceived: null client");
            else if (command != null)
            {
                if (IsAsynchronous)
                {
                    if (playerClient.Paging.HasPageLeft) // if paging, valid commands are <Enter>, Quit, All
                        HandlePaging(playerClient, command);
                    else
                        playerClient.Player.ProcessCommand(command);
                }
                else
                    playerClient.EnqueueReceivedData(command);
            }
        }

        private void PlayerOnSendData(IPlayer player, string data)
        {
            PlayerClient playerClient;
            bool found = _players.TryGetValue(player, out playerClient);
            //PlayerClient playerClient = _playerClients.FirstOrDefault(x => x.Player == player);
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

        private void PlayerOnPageData(IPlayer player, StringBuilder data)
        {
            PlayerClient playerClient;
            bool found = _players.TryGetValue(player, out playerClient);
            //PlayerClient playerClient = _playerClients.FirstOrDefault(x => x.Player == player);
            if (playerClient == null)
                Log.Default.WriteLine(LogLevels.Error, "PlayerOnPageData: null client");
            else if (data.Length > 0)
            {
                // Save data to page
                playerClient.Paging.SetData(data); // doesn't depend on asynchronous or synchronous
                // Send first page
                HandlePaging(playerClient, String.Empty);
            }
        }

        #endregion

        private void HandlePaging(PlayerClient playerClient, string command)
        {
            if (command == String.Empty) // <Enter> -> send next page
            {
                string nextPage = playerClient.Paging.GetNextPage(5); // TODO: configurable line count
                playerClient.Client.WriteData(nextPage); // TODO: use PlayerOnSendData ???
                if (playerClient.Paging.HasPageLeft) // page left, send page instructions
                {
                    const string pagingInstructions = "[Paging : (Enter), (Q)uit, (A)ll]";
                    if (IsAsynchronous)
                        playerClient.Client.WriteData(pagingInstructions);
                    else
                        playerClient.EnqueueDataToSend(pagingInstructions);
                }
                else // no more page -> normal mode
                {
                    // TODO: problem in synchronous mode, 2 prompts are displayed
                    const string prompt = ">"; // TODO: complex prompt
                    if (IsAsynchronous)
                        playerClient.Client.WriteData(prompt);
                    else
                        playerClient.EnqueueDataToSend(prompt);
                }
            }
            else if ("quit".StartsWith(command.ToLower()))
            {
                playerClient.Paging.Clear();
                const string prompt = ">"; // TODO: complex prompt
                if (IsAsynchronous)
                    playerClient.Client.WriteData(prompt);
                else
                    playerClient.EnqueueDataToSend(prompt);
            }
            else if ("all".StartsWith(command.ToLower()))
            {
                string remaining = playerClient.Paging.GetRemaining();
                if (IsAsynchronous)
                {
                    const string prompt = ">"; // TODO: complex prompt
                    playerClient.Client.WriteData(remaining);
                    playerClient.Client.WriteData(prompt);
                }
                else
                    playerClient.EnqueueDataToSend(remaining); // no need to bust a prompt because ProcessOutput will do it
            }
        }

        private void Broadcast(string message)
        {
            message = message + Environment.NewLine;
            // By-pass asynchronous/synchronous send
            //foreach (PlayerClient playerClient in _playerClients)
            //  playerClient.Client.WriteData(message);
            foreach (IClient client in _clients.Keys)
                client.WriteData(message);
        }

        private void ProcessInput()
        {
            // TODO: if paging

            // Read one command from each client and process it
            if (!IsAsynchronous)
            {
                //foreach (PlayerClient playerClient in _playerClients) // TODO: first connected player will be processed before other, try a randomize
                //{
                //    string data = playerClient.DequeueReceivedData(); // process one command at a time
                //    if (!String.IsNullOrWhiteSpace(data))
                //        playerClient.Player.ProcessCommand(data);
                //}
                foreach (PlayerClient playerClient in _players.Values)
                {
                    string command = playerClient.DequeueReceivedData(); // process one command at a time
                    if (command != null)
                    {
                        if (playerClient.Paging.HasPageLeft) // if paging, valid commands are <Enter>, Quit, All
                            HandlePaging(playerClient, command);
                        else if (!String.IsNullOrWhiteSpace(command))
                            playerClient.Player.ProcessCommand(command);
                    }
                }
            }
        }

        private void ProcessOutput()
        {
            // TODO: if paging

            if (!IsAsynchronous)
            {
                //foreach (PlayerClient playerClient in _playerClients)
                foreach (PlayerClient playerClient in _players.Values)
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
                    if (!String.IsNullOrWhiteSpace(data)) // TODO use stringbuilder to append prompt
                    {
                        // Bust a prompt ?
                        if (playerClient.Player.PlayerState == PlayerStates.Connected || playerClient.Player.PlayerState == PlayerStates.Playing)
                            data += ">"; // TODO: complex prompt
                        playerClient.Client.WriteData(data);
                    }
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
