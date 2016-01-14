using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mud.Logger;
using Mud.Network;
using Mud.Server.Constants;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Server
{
    // Player lifecycle:
    //  when INetworkServer detects a new connection, NewClientConnected is raised
    //  a new login state machine is created/started and associated to client, client inputs/outputs are handled by login state machine instead if ProcessInput (via ClientLoginOnDataReceived)
    //      --> client is considered as connecting
    //  if login is failed, client is disconnected
    //  if login is successful, login state machine is discarded, player/admin is created and client input/outputs are handled with ProcessInput/ProcessOutput
    //      --> client is considered as playing

    // Once playing,
    //  in synchronous mode, input and output are 'queued' and handled by ProcessorInput/ProcessOutput
    public class Server : IServer
    {
        // This allows fast lookup with client or player BUT both structures must be modified at the same time
        private readonly object _playingClientLockObject = new object();
        private readonly ConcurrentDictionary<IClient, PlayingClient> _clients;
        private readonly ConcurrentDictionary<IPlayer, PlayingClient> _players;

        // Client in login process are not yet considered as player, they are stored in a seperate stucture
        private readonly ConcurrentDictionary<IClient, LoginStateMachine> _loginInClients;

        private List<INetworkServer> _networkServers;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _gameLoopTask;

        private volatile int _pulseBeforeShutdown; // pulse count before shutdown
        private int _pulseViolence;

        #region Singleton

        private static readonly Lazy<Server> Lazy = new Lazy<Server>(() => new Server());

        public static IServer Instance
        {
            get { return Lazy.Value; }
        }

        private Server()
        {
            _clients = new ConcurrentDictionary<IClient, PlayingClient>();
            _players = new ConcurrentDictionary<IPlayer, PlayingClient>();
            _loginInClients = new ConcurrentDictionary<IClient, LoginStateMachine>();
        }

        #endregion

        #region IServer

        public DateTime CurrentTime { get; private set; }

        public void Initialize(List<INetworkServer> networkServers)
        {
            _networkServers = networkServers;
            foreach (INetworkServer networkServer in _networkServers)
            {
                networkServer.NewClientConnected += NetworkServerOnNewClientConnected;
                networkServer.ClientDisconnected += NetworkServerOnClientDisconnected;
            }
        }

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _gameLoopTask = Task.Factory.StartNew(GameLoopTask, _cancellationTokenSource.Token);

            foreach(INetworkServer networkServer in _networkServers)
            {
                networkServer.Initialize();
                networkServer.Start();
            }
        }

        public void Stop()
        {
            try
            {
                foreach (INetworkServer networkServer in _networkServers)
                {
                    networkServer.Stop();
                    networkServer.NewClientConnected -= NetworkServerOnNewClientConnected;
                    networkServer.ClientDisconnected -= NetworkServerOnClientDisconnected;
                }

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
            _pulseBeforeShutdown = seconds * ServerOptions.PulsePerSeconds;
        }

        public void Quit(IPlayer player)
        {
            PlayingClient playingClient;
            _players.TryGetValue(player, out playingClient);
            if (playingClient == null)
                Log.Default.WriteLine(LogLevels.Error, "Quit: client not found");
            else
                ClientPlayingOnDisconnected(playingClient.Client);
        }

        public IPlayer GetPlayer(CommandParameter parameter, bool perfectMatch)
        {
            return FindHelpers.FindByName(_players.Keys, parameter, perfectMatch);
        }

        public IReadOnlyCollection<IPlayer> GetPlayers()
        {
            return _players.Keys.ToList().AsReadOnly();
        }

        public IAdmin GetAdmin(CommandParameter parameter, bool perfectMatch)
        {
            return FindHelpers.FindByName(_players.Keys.OfType<IAdmin>(), parameter, perfectMatch);
        }

        public IReadOnlyCollection<IAdmin> GetAdmins()
        {
            return _players.Keys.OfType<IAdmin>().ToList().AsReadOnly();
        }

        // TODO: remove
        // TEST PURPOSE
        public IPlayer AddPlayer(IClient client, string name)
        {
            IPlayer player = new Player.Player(Guid.NewGuid(), name);
            player.SendData += PlayerOnSendData;
            player.PageData += PlayerOnPageData;
            client.DataReceived += ClientPlayingOnDataReceived;
            PlayingClient playingClient = new PlayingClient
            {
                Client = client,
                Player = player
            };
            lock (_playingClientLockObject)
            {
                _players.TryAdd(player, playingClient);
                _clients.TryAdd(client, playingClient);
            }

            player.Send("Welcome {0}" + Environment.NewLine, name);

            return player;
        }

        public IAdmin AddAdmin(IClient client, string name)
        {
            IAdmin admin = new Admin.Admin(Guid.NewGuid(), name);
            admin.SendData += PlayerOnSendData;
            admin.PageData += PlayerOnPageData;
            client.DataReceived += ClientPlayingOnDataReceived;
            PlayingClient playingClient = new PlayingClient
            {
                Client = client,
                Player = admin
            };
            lock (_playingClientLockObject)
            {
                _players.TryAdd(admin, playingClient);
                _clients.TryAdd(client, playingClient);
            }

            admin.Send("Welcome master {0}" + Environment.NewLine, name);

            return admin;
        }

        #endregion

        #region Event handlers

        private void NetworkServerOnNewClientConnected(IClient client)
        {
            Log.Default.WriteLine(LogLevels.Info, "NetworkServerOnNewClientConnected");
            // Create/store a login state machine and starts it
            LoginStateMachine loginStateMachine = new LoginStateMachine();
            _loginInClients.TryAdd(client, loginStateMachine);
            // Add login handlers
            loginStateMachine.LoginFailed += LoginStateMachineOnLoginFailed;
            loginStateMachine.LoginSuccessful += LoginStateMachineOnLoginSuccessful;
            client.DataReceived += ClientLoginOnDataReceived;
            // Send greetings
            client.WriteData("Why don't you login or tell us the name you wish to be known by?");
        }

        private void NetworkServerOnClientDisconnected(IClient client)
        {
            Log.Default.WriteLine(LogLevels.Info, "NetworkServerOnClientDisconnected");
            LoginStateMachine loginStateMachine;
            _loginInClients.TryRemove(client, out loginStateMachine);
            if (loginStateMachine != null) // no player yet, disconnected while log in
            {
                loginStateMachine.LoginFailed -= LoginStateMachineOnLoginFailed;
                loginStateMachine.LoginSuccessful -= LoginStateMachineOnLoginSuccessful;
                client.DataReceived -= ClientLoginOnDataReceived;
            }
            else // log in passed, player exists
            {
                client.DataReceived -= ClientPlayingOnDataReceived;
                ClientPlayingOnDisconnected(client);
            }
        }

        private void ClientLoginOnDataReceived(IClient client, string command)
        {
            LoginStateMachine loginStateMachine;
            _loginInClients.TryGetValue(client, out loginStateMachine);
            if (loginStateMachine != null)
                loginStateMachine.ProcessInput(client, command);
            else
                Log.Default.WriteLine(LogLevels.Error, "ClientLoginOnDataReceived: LoginStateMachine not found for a client!!!");
        }

        private void LoginStateMachineOnLoginSuccessful(IClient client, string username, bool isAdmin, bool isNewPlayer)
        {
            Log.Default.WriteLine(LogLevels.Info, "LoginStateMachineOnLoginSuccessful");

            // TODO: if new player, avatar creation state machine
            
            client.WriteData("Welcome to Mud.Net!!" + Environment.NewLine);
            client.WriteData(">"); // TODO: complex prompt

            // Remove login state machine
            LoginStateMachine loginStateMachine;
            _loginInClients.TryRemove(client, out loginStateMachine);
            if (loginStateMachine != null)
            {
                loginStateMachine.LoginFailed -= LoginStateMachineOnLoginFailed;
                loginStateMachine.LoginSuccessful -= LoginStateMachineOnLoginSuccessful;
            }
            else
                Log.Default.WriteLine(LogLevels.Error, "LoginStateMachineOnLoginSuccessful: LoginStateMachine not found for a client!!!");

            IPlayer playerOrAdmin;
            if (isAdmin)
                playerOrAdmin = new Admin.Admin(Guid.NewGuid(), username);
            else
            {
                playerOrAdmin = new Player.Player(Guid.NewGuid(), username);
            }
            // Remove login handlers
            client.DataReceived -= ClientLoginOnDataReceived;
            // Add playing handlers
            client.DataReceived += ClientPlayingOnDataReceived;
            //
            playerOrAdmin.SendData += PlayerOnSendData;
            playerOrAdmin.PageData += PlayerOnPageData;
            PlayingClient playingClient = new PlayingClient
            {
                Client = client,
                Player = playerOrAdmin
            };
            lock (_playingClientLockObject)
            {
                _players.TryAdd(playerOrAdmin, playingClient);
                _clients.TryAdd(client, playingClient);
            }

            // Load player
            playerOrAdmin.Load(username);
        }

        public void LoginStateMachineOnLoginFailed(IClient client)
        {
            // TODO: remove login state machine and disconnect client
        }

        private void ClientPlayingOnDataReceived(IClient client, string command)
        {
            PlayingClient playingClient;
            lock(_playingClientLockObject)
                _clients.TryGetValue(client, out playingClient);
            if (playingClient == null)
                Log.Default.WriteLine(LogLevels.Error, "ClientPlayingOnDataReceived: null client");
            else if (command != null)
                playingClient.EnqueueReceivedData(command);
        }

        private void ClientPlayingOnDisconnected(IClient client)
        {
            Log.Default.WriteLine(LogLevels.Info, "ClientPlayingOnDisconnected");

            PlayingClient playingClient;
            bool removed;
            lock (_playingClientLockObject)
            {
                removed = _clients.TryRemove(client, out playingClient);
                if (removed)
                    _players.TryRemove(playingClient.Player, out playingClient);
                // !!! PlayingClient removed from both collection must be equal
            }

            if (!removed)
                Log.Default.WriteLine(LogLevels.Error, "ClientPlayingOnDisconnected: playingClient not found!!!");
            else
            {
                playingClient.Player.OnDisconnected();
                client.DataReceived -= ClientPlayingOnDataReceived;
                playingClient.Player.SendData -= PlayerOnSendData;
                playingClient.Player.PageData -= PlayerOnPageData;
            }
        }

        private void PlayerOnSendData(IPlayer player, string data)
        {
            PlayingClient playingClient;
            _players.TryGetValue(player, out playingClient);
            if (playingClient == null)
                Log.Default.WriteLine(LogLevels.Error, "PlayerOnSendData: playingClient not found!!!");
            else
                playingClient.EnqueueDataToSend(data);
        }

        private void PlayerOnPageData(IPlayer player, StringBuilder data)
        {
            PlayingClient playingClient;
            bool found = _players.TryGetValue(player, out playingClient);
            if (playingClient == null)
                Log.Default.WriteLine(LogLevels.Error, "PlayerOnPageData: playingClient not found!!!");
            else if (data.Length > 0)
            {
                // Save data to page
                playingClient.Paging.SetData(data);
                // Send first page
                HandlePaging(playingClient, String.Empty);
            }
        }

        #endregion

        // Once paging is active, classic commands are processed anymore
        // Valid commands are (Enter), (Q)uit, (A)ll
        // TODO: (N)ext same as Enter  (P)revious
        private void HandlePaging(PlayingClient playingClient, string command)
        {
            if (command == String.Empty) // <Enter> -> send next page
            {
                // Pages are always sent immediately asynchronously, don't use ProcessOutput even if in synchronous mode
                string nextPage = playingClient.Paging.GetNextPage(20); // TODO: configurable line count
                playingClient.Client.WriteData(nextPage);
                if (playingClient.Paging.HasPageLeft) // page left, send page instructions
                {
                    const string pagingInstructions = "[Paging : (Enter), (Q)uit, (A)ll]";
                    playingClient.Client.WriteData(pagingInstructions);
                }
                else // no more page -> normal mode
                {
                    playingClient.Paging.Clear();
                    const string prompt = ">"; // TODO: complex prompt
                    playingClient.Client.WriteData(prompt);
                }
            }
            else if ("quit".StartsWith(command.ToLower()))
            {
                playingClient.Paging.Clear();
                const string prompt = ">"; // TODO: complex prompt
                playingClient.Client.WriteData(prompt);
            }
            else if ("all".StartsWith(command.ToLower()))
            {
                string remaining = playingClient.Paging.GetRemaining();
                playingClient.Paging.Clear();
                const string prompt = ">"; // TODO: complex prompt
                playingClient.Client.WriteData(remaining);
                playingClient.Client.WriteData(prompt);
            }
        }

        private void Broadcast(string message)
        {
            message = message + Environment.NewLine;
            // By-pass process output
            lock(_playingClientLockObject)
                foreach (IClient client in _clients.Keys)
                    client.WriteData(message);
        }

        private void ProcessInput()
        {
            // Read one command from each client and process it
            foreach (PlayingClient playingClient in _players.Values) // TODO: first connected player will be processed before other, try a randomize
            {
                if (playingClient.Player != null)
                {
                    if (playingClient.Player.GlobalCooldown > 0) // if player is on GCD, decrease it
                        playingClient.Player.DecreaseGlobalCooldown();
                    else
                    {
                        string command = playingClient.DequeueReceivedData(); // process one command at a time
                        if (command != null)
                        {
                            if (playingClient.Paging.HasPageLeft) // if paging, valid commands are <Enter>, Quit, All
                                HandlePaging(playingClient, command);
                            else if (!String.IsNullOrWhiteSpace(command))
                            {
                                try
                                {
                                    playingClient.Player.ProcessCommand(command);
                                }
                                catch (Exception ex)
                                {
                                    Log.Default.WriteLine(LogLevels.Error, "Exception while processing input of {0}. Exception: {1}", playingClient.Player.Name, ex);
                                }
                            }
                        }
                    }
                }
                else
                    Log.Default.WriteLine(LogLevels.Error, "ProcessInput: playing client without Player");
            }
        }

        private void ProcessOutput()
        {
            foreach (PlayingClient playingClient in _players.Values)
            {
                if (playingClient.Player != null)
                {
                    string data = playingClient.DequeueDataToSend();
                    try
                    {
                        if (!String.IsNullOrWhiteSpace(data)) // TODO use stringbuilder to append prompt
                        {
                            // Bust a prompt ?
                            if (playingClient.Player.PlayerState == PlayerStates.Playing || playingClient.Player.PlayerState == PlayerStates.Impersonating)
                            {
                                // bust a prompt // TODO: complex prompt
                                if (playingClient.Player.Impersonating != null)
                                    data += String.Format("<{0}/{1}hp>", playingClient.Player.Impersonating.HitPoints, playingClient.Player.Impersonating[ComputedAttributeTypes.MaxHitPoints]);
                                else
                                    data += ">";
                            }
                            else
                                data += ">";
                            playingClient.Client.WriteData(data);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Default.WriteLine(LogLevels.Error, "Exception while processing output of {0}. Exception: {1}", playingClient.Player.Name, ex);
                    }
                }
                else
                    Log.Default.WriteLine(LogLevels.Error, "ProcessOutput: playing client without Player");
            }
        }

        private void HandleShutdown()
        {
            if (_pulseBeforeShutdown >= 0)
            {
                _pulseBeforeShutdown--;
                if (_pulseBeforeShutdown == ServerOptions.PulsePerSeconds * 60 * 15)
                    Broadcast("%R%Shutdown in 15 minutes%x%");
                if (_pulseBeforeShutdown == ServerOptions.PulsePerSeconds * 60 * 10)
                    Broadcast("%R%Shutdown in 10 minutes%x%");
                if (_pulseBeforeShutdown == ServerOptions.PulsePerSeconds * 60 * 5)
                    Broadcast("%R%Shutdown in 5 minutes%x%");
                if (_pulseBeforeShutdown == ServerOptions.PulsePerSeconds * 60)
                    Broadcast("%R%Shutdown in 1 minute%x%");
                if (_pulseBeforeShutdown == ServerOptions.PulsePerSeconds * 30)
                    Broadcast("%R%Shutdown in 30 seconds%x%");
                if (_pulseBeforeShutdown == ServerOptions.PulsePerSeconds * 15)
                    Broadcast("%R%Shutdown in 15 seconds%x%");
                if (_pulseBeforeShutdown == ServerOptions.PulsePerSeconds * 10)
                    Broadcast("%R%Shutdown in 10 seconds%x%");
                if (_pulseBeforeShutdown == ServerOptions.PulsePerSeconds * 5)
                    Broadcast("%R%Shutdown in 5%x%");
                if (_pulseBeforeShutdown == ServerOptions.PulsePerSeconds * 4)
                    Broadcast("%R%Shutdown in 4%x%");
                if (_pulseBeforeShutdown == ServerOptions.PulsePerSeconds * 3)
                    Broadcast("%R%Shutdown in 3%x%");
                if (_pulseBeforeShutdown == ServerOptions.PulsePerSeconds * 2)
                    Broadcast("%R%Shutdown in 2%x%");
                if (_pulseBeforeShutdown == ServerOptions.PulsePerSeconds * 1)
                    Broadcast("%R%Shutdown in 1%x%");
                else if (_pulseBeforeShutdown == 0)
                {
                    Broadcast("%R%Shutdown NOW!!!%x%");
                    Stop();
                }
            }
        }

        private void HandlePeriodicAuras() // TODO: specific pulse ? 1/2 seconds
        {
            // TODO: remove aura with amount == 0 ?
            // Remove dot/hot on non-impersonated if source is not the in same room (or source is inexistant)
            // TODO: take periodic aura that will be processed/removed
            IReadOnlyCollection<ICharacter> clonePeriodicAuras = new ReadOnlyCollection<ICharacter>(World.World.Instance.GetCharacters().Where(x => x.PeriodicAuras.Any()).ToList());
            foreach (ICharacter character in clonePeriodicAuras)
            {
                try
                {
                    IReadOnlyCollection<IPeriodicAura> periodicAuras = new ReadOnlyCollection<IPeriodicAura>(character.PeriodicAuras.ToList());
                    foreach (IPeriodicAura pa in periodicAuras)
                    {
                        // On NPC, remove hot/dot from unknown source or source not in the same room
                        if (character.ImpersonatedBy == null && (pa.Source == null || pa.Source.Room != character.Room))
                            character.RemovePeriodicAura(pa);
                        else // Otherwise, process normally
                        {
                            if (pa.TicksLeft > 0)
                                pa.Process(character);
                            if (pa.TicksLeft == 0) // no else, because Process decrease PeriodsLeft
                                character.RemovePeriodicAura(pa);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Exception while handling periodic auras of {0}. Exception: {1}", character.Name, ex);
                }
            }
        }

        private void HandleAuras() // TODO: specific pulse ? 1/2 seconds
        {
            // TODO: remove aura with amount == 0 ?
            // Take aura that will expired
            IReadOnlyCollection<ICharacter> cloneAuras = new ReadOnlyCollection<ICharacter>(World.World.Instance.GetCharacters().Where(x => x.Auras.Any(b => b.SecondsLeft <= 0)).ToList());
            foreach (ICharacter character in cloneAuras)
            {
                try
                {
                    IReadOnlyCollection<IAura> auras = new ReadOnlyCollection<IAura>(character.Auras.ToList());
                    bool needsRecompute = false;
                    foreach (IAura aura in auras.Where(x => x.SecondsLeft <= 0))
                    {
                        character.RemoveAura(aura, false); // recompute once each aura has been processed
                        needsRecompute = true;
                    }
                    if (needsRecompute)
                        character.RecomputeAttributes();
                }
                catch (Exception ex)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Exception while handling auras of {0}. Exception: {1}", character.Name, ex);
                }
            }
        }

        private void HandleViolence()
        {
            if (_pulseViolence > 0)
            {
                _pulseViolence--;
                return;
            }
            _pulseViolence = ServerOptions.PulseViolence;

            Log.Default.WriteLine(LogLevels.Trace, "PulseViolence");

            IReadOnlyCollection<ICharacter> clone = new ReadOnlyCollection<ICharacter>(World.World.Instance.GetCharacters().Where(x => x.Fighting != null).ToList());
            foreach (ICharacter character in clone)
            {
                ICharacter victim = character.Fighting;
                if (victim != null)
                {
                    try
                    {
                        if (victim.Room == character.Room) // fight continue only if in the same room
                        {
                            Log.Default.WriteLine(LogLevels.Debug, "Continue fight between {0} and {1}", character.Name, victim.Name);
                            character.MultiHit(victim);
                        }
                        else
                        {
                            Log.Default.WriteLine(LogLevels.Debug, "Stop fighting between {0} and {1}, because not in same room", character.Name, victim.Name);
                            character.StopFighting(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Default.WriteLine(LogLevels.Error, "Exception while handling violence of {0}. Exception: {1}", character.Name, ex);
                    }
                }
            }
        }

        private void DoPulse()
        {
            // TODO:  (see handler.c)
            //Log.Default.WriteLine(LogLevels.Debug, "PULSE: {0:HH:mm:ss.ffffff}", DateTime.Now);

            HandleShutdown();
            HandlePeriodicAuras();
            HandleAuras();
            HandleViolence();
        }

        private void GameLoopTask()
        {
            _pulseViolence = ServerOptions.PulseViolence;

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

                    CurrentTime = DateTime.Now;

                    ProcessInput();

                    DoPulse();

                    ProcessOutput();

                    // TODO: remove !IsValid entity

                    sw.Stop();
                    long elapsedMs = sw.ElapsedMilliseconds; // in milliseconds
                    if (elapsedMs < ServerOptions.PulseDelay)
                    {
                        //long elapsedTick = sw.ElapsedTicks; // 1 tick = 1 second/Stopwatch.Frequency
                        //long elapsedNs = sw.Elapsed.Ticks; // 1 tick = 1 nanosecond
                        //Log.Default.WriteLine(LogLevels.Debug, "Elapsed {0}Ms {1}Ticks {2}Ns", elapsedMs, elapsedTick, elapsedNs);
                        //Thread.Sleep(250 - (int) elapsedMs);
                        int sleepTime = ServerOptions.PulseDelay - (int)elapsedMs;
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
