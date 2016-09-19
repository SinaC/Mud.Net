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
using Mud.Server.Abilities;
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
    public class Server : IServer, IDisposable
    {
        // This allows fast lookup with client or player BUT both structures must be modified at the same time
        private readonly object _playingClientLockObject = new object();
        private readonly ConcurrentDictionary<IClient, PlayingClient> _clients;
        private readonly ConcurrentDictionary<IPlayer, PlayingClient> _players;

        // Client in login process are not yet considered as player, they are stored in a seperate structure
        private readonly ConcurrentDictionary<IClient, LoginStateMachine> _loginInClients;

        private List<INetworkServer> _networkServers;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _gameLoopTask;

        private volatile int _pulseBeforeShutdown; // pulse count before shutdown

        #region Singleton

        private static readonly Lazy<Server> Lazy = new Lazy<Server>(() => new Server());

        public static IServer Instance => Lazy.Value;

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

            // Perform some validity/sanity checks
            foreach (IClass c in Repository.ClassManager.Classes)
            {
                if (c.ResourceKinds == null || !c.ResourceKinds.Any())
                    Log.Default.WriteLine(LogLevels.Warning, "Class {0} doesn't have any allowed resources");
                else
                {
                    foreach (AbilityAndLevel abilityAndLevel in c.Abilities)
                        if (abilityAndLevel.Ability.ResourceKind != ResourceKinds.None && !c.ResourceKinds.Contains(abilityAndLevel.Ability.ResourceKind))
                            Log.Default.WriteLine(LogLevels.Warning, "Class {0} is allowed to use ability {1} [resource:{2}] but doesn't have access to that resource", c.DisplayName, abilityAndLevel.Ability.Name, abilityAndLevel.Ability.ResourceKind);
                }
            }
            long totalExperience = 0;
            long previousExpToLevel = 0;
            for (int lvl = 1; lvl < 100; lvl++)
            {
                long expToLevel;
                bool found = CombatHelpers.ExperienceToNextLevel.TryGetValue(lvl, out expToLevel);
                if (!found)
                    Log.Default.WriteLine(LogLevels.Error, "No experience to next level found for level {0}", lvl);
                else if (expToLevel < previousExpToLevel)
                    Log.Default.WriteLine(LogLevels.Error, "Experience to next level for level {0} is lower than previous level", lvl);
                else
                    previousExpToLevel = expToLevel;
                totalExperience += expToLevel;
            }
            Log.Default.WriteLine(LogLevels.Info, "Total experience from 1 to 100 = {0:n0}", totalExperience);
        }

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _gameLoopTask = Task.Factory.StartNew(GameLoopTask, _cancellationTokenSource.Token);

            foreach (INetworkServer networkServer in _networkServers)
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
                Broadcast($"%R%Shutdown in {minutes} minute{(minutes > 1 ? "s" : String.Empty)} and {remaining} second{(remaining > 1 ? "s" : String.Empty)}%x%");
            else if (minutes > 0 && remaining == 0)
                Broadcast($"%R%Shutdown in {minutes} minute{(minutes > 1 ? "s" : String.Empty)}%x%");
            else
                Broadcast($"%R%Shutdown in {seconds} second{(seconds > 1 ? "s" : String.Empty)}%x%");
            _pulseBeforeShutdown = seconds*ServerOptions.PulsePerSeconds;
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

        public void Wiznet(string message, WiznetFlags flags, AdminLevels minLevel = AdminLevels.Angel)
        {
            foreach (IAdmin admin in Admins.Where(a => (a.WiznetFlags & flags) == flags && a.Level >= minLevel))
                admin.Send($"%W%WIZNET%x%:{message}");
        }

        public IPlayer GetPlayer(CommandParameter parameter, bool perfectMatch)
        {
            return FindHelpers.FindByName(_players.Keys, parameter, perfectMatch);
        }

        public IEnumerable<IPlayer> Players => _players.Keys;

        public IAdmin GetAdmin(CommandParameter parameter, bool perfectMatch)
        {
            return FindHelpers.FindByName(_players.Keys.OfType<IAdmin>(), parameter, perfectMatch);
        }

        public IEnumerable<IAdmin> Admins => _players.Keys.OfType<IAdmin>();

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

            player.Send("Welcome {0}", name);

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

            admin.Send("Welcome master {0}", name);

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
            //// TODO: TEST purpose
            //client.WriteData("%y%Quest Quest 1: the beggar          :   1 /   3 (33%)%x%");
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
                if (client.IsConnected)
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

            IPlayer playerOrAdmin = null;
            // if same user is already connected, remove old client and link new client to old player
            KeyValuePair<IPlayer, PlayingClient> previousPlayerPair;
            lock (_playingClientLockObject)
            {
                previousPlayerPair = _players.FirstOrDefault(x => x.Key.Name == username);
            }
            if (previousPlayerPair.Key != null)
            {
                Log.Default.WriteLine(LogLevels.Info, "Player was already connected, disconnect previous client and reuse player");

                // Save player
                playerOrAdmin = previousPlayerPair.Key; // TODO: pause client ????
                // Remove client and player from players/clients
                lock (_playingClientLockObject)
                {
                    PlayingClient oldPlayingClient;
                    bool removed = _players.TryRemove(playerOrAdmin, out oldPlayingClient);
                    if (removed)
                        _clients.TryRemove(oldPlayingClient.Client, out oldPlayingClient);
                    // !!! PlayingClient removed from both collection must be equal
                }

                // Disconnect previous client
                previousPlayerPair.Value.Client.WriteData("Reconnecting on another client!!");
                previousPlayerPair.Value.Client.DataReceived -= ClientPlayingOnDataReceived;
                previousPlayerPair.Value.Client.Disconnect();

                Wiznet($"{username} has reconnected.", WiznetFlags.Logins);

                // Welcome back
                client.WriteData("Reconnecting to Mud.Net!!" + Environment.NewLine);
            }
            else
            {
                Wiznet($"{username} has connected.", WiznetFlags.Logins);

                // Welcome
                client.WriteData("Welcome to Mud.Net!!" + Environment.NewLine);
            }
            // TODO: if new player, avatar creation state machine

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

            // Remove login handlers
            client.DataReceived -= ClientLoginOnDataReceived;
            // Add playing handlers
            client.DataReceived += ClientPlayingOnDataReceived;

            bool loadPlayerOrAdmin = false;
            // Create a new player/admin only if not reconnecting
            if (playerOrAdmin == null)
            {
                playerOrAdmin = isAdmin 
                    ? new Admin.Admin(Guid.NewGuid(), username) 
                    : new Player.Player(Guid.NewGuid(), username);
                //
                playerOrAdmin.SendData += PlayerOnSendData;
                playerOrAdmin.PageData += PlayerOnPageData;
                loadPlayerOrAdmin = true;
            }
            //
            PlayingClient newPlayingClient = new PlayingClient
            {
                Client = client,
                Player = playerOrAdmin
            };
            lock (_playingClientLockObject)
            {
                _players.TryAdd(playerOrAdmin, newPlayingClient);
                _clients.TryAdd(client, newPlayingClient);
            }

            // Prompt
            client.WriteData(playerOrAdmin.Prompt);

            // Load player/admin (if needed)
            if (loadPlayerOrAdmin)
                playerOrAdmin.Load(username);
        }

        public void LoginStateMachineOnLoginFailed(IClient client)
        {
            // TODO: remove login state machine and disconnect client
        }

        private void ClientPlayingOnDataReceived(IClient client, string command)
        {
            PlayingClient playingClient;
            lock (_playingClientLockObject)
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
                Wiznet($"{playingClient.Player.DisplayName} has disconnected.", WiznetFlags.Logins);

                IAdmin admin = playingClient.Player as IAdmin;
                // Remove nullify LastTeller and SnoopBy
                foreach (IPlayer player in Players)
                {
                    if (player.LastTeller == playingClient.Player)
                        player.SetLastTeller(null);
                    if (admin != null && player.SnoopBy == admin)
                        player.SetSnoopBy(null);
                }
                playingClient.Player.OnDisconnected();
                playingClient.Client.Disconnect();
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
            if (!found || playingClient == null)
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
        // Valid commands are (Enter), (N)ext, (Q)uit, (A)ll
        // TODO: (P)revious
        private void HandlePaging(PlayingClient playingClient, string command)
        {
            string lowerCommand = command.ToLowerInvariant();
            if (command == String.Empty || "next".StartsWith(lowerCommand)) // <Enter> or Next -> send next page
            {
                // Pages are always sent immediately asynchronously, don't use ProcessOutput even if in synchronous mode
                string nextPage = playingClient.Paging.GetNextPage(25); // TODO: configurable line count
                playingClient.Client.WriteData(nextPage);
                if (playingClient.Paging.HasPageLeft) // page left, send page instructions (no prompt)
                    playingClient.Client.WriteData(ServerOptions.PagingInstructions);
                else // no more page -> normal mode
                {
                    playingClient.Paging.Clear();
                    playingClient.Client.WriteData(playingClient.Player.Prompt);
                }
            }
            else if ("previous".StartsWith(lowerCommand))
            {
                string previousPage = playingClient.Paging.GetPreviousPage(25); // TODO: configurable line count
                playingClient.Client.WriteData(previousPage);
                if (playingClient.Paging.HasPageLeft) // page left, send page instructions (no prompt)
                    playingClient.Client.WriteData(ServerOptions.PagingInstructions);
                else // no more page -> normal mode
                {
                    playingClient.Paging.Clear();
                    playingClient.Client.WriteData(playingClient.Player.Prompt);
                }
            }
            else if ("quit".StartsWith(lowerCommand))
            {
                playingClient.Paging.Clear();
                playingClient.Client.WriteData(Environment.NewLine);
                playingClient.Client.WriteData(playingClient.Player.Prompt);
            }
            else if ("all".StartsWith(lowerCommand))
            {
                string remaining = playingClient.Paging.GetRemaining();
                playingClient.Paging.Clear();
                playingClient.Client.WriteData(remaining);
                playingClient.Client.WriteData(playingClient.Player.Prompt);
            }
        }

        private void Broadcast(string message)
        {
            message = message + Environment.NewLine;
            // By-pass process output
            lock (_playingClientLockObject)
                foreach (IClient client in _clients.Keys)
                    client.WriteData(message);
        }

        private void ProcessInput()
        {
            // Read one command from each client and process it
            foreach (PlayingClient playingClient in _players.Values) // TODO: first connected player will be processed before other, try a randomize (round-robin)
            {
                if (playingClient.Player != null)
                {
                    try
                    {
                        if (playingClient.Player.GlobalCooldown > 0) // if player is on GCD, decrease it
                            playingClient.Player.DecreaseGlobalCooldown();
                        else
                        {
                            string command = playingClient.DequeueReceivedData(); // process one command at a time
                            if (command != null)
                            {
                                if (playingClient.Paging.HasPageLeft) // if paging, valid commands are <Enter>, Next, Quit, All
                                    HandlePaging(playingClient, command);
                                else if (!String.IsNullOrWhiteSpace(command))
                                {
                                    playingClient.Player.ProcessCommand(command);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Default.WriteLine(LogLevels.Error, "Exception while processing input of {0}. Exception: {1}", playingClient.Player.Name, ex);
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
                    try
                    {
                        string datas = playingClient.DequeueDataToSend(); // TODO should return a StringBuilder to quickly append prompt
                        if (!String.IsNullOrWhiteSpace(datas))
                        {
                            // Add prompt
                            datas += playingClient.Player.Prompt;
                            // Send datas
                            playingClient.Client.WriteData(datas);
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
                if (_pulseBeforeShutdown == ServerOptions.PulsePerSeconds*60*15)
                    Broadcast("%R%Shutdown in 15 minutes%x%");
                if (_pulseBeforeShutdown == ServerOptions.PulsePerSeconds*60*10)
                    Broadcast("%R%Shutdown in 10 minutes%x%");
                if (_pulseBeforeShutdown == ServerOptions.PulsePerSeconds*60*5)
                    Broadcast("%R%Shutdown in 5 minutes%x%");
                if (_pulseBeforeShutdown == ServerOptions.PulsePerSeconds*60)
                    Broadcast("%R%Shutdown in 1 minute%x%");
                if (_pulseBeforeShutdown == ServerOptions.PulsePerSeconds*30)
                    Broadcast("%R%Shutdown in 30 seconds%x%");
                if (_pulseBeforeShutdown == ServerOptions.PulsePerSeconds*15)
                    Broadcast("%R%Shutdown in 15 seconds%x%");
                if (_pulseBeforeShutdown == ServerOptions.PulsePerSeconds*10)
                    Broadcast("%R%Shutdown in 10 seconds%x%");
                if (_pulseBeforeShutdown == ServerOptions.PulsePerSeconds*5)
                    Broadcast("%R%Shutdown in 5%x%");
                if (_pulseBeforeShutdown == ServerOptions.PulsePerSeconds*4)
                    Broadcast("%R%Shutdown in 4%x%");
                if (_pulseBeforeShutdown == ServerOptions.PulsePerSeconds*3)
                    Broadcast("%R%Shutdown in 3%x%");
                if (_pulseBeforeShutdown == ServerOptions.PulsePerSeconds*2)
                    Broadcast("%R%Shutdown in 2%x%");
                if (_pulseBeforeShutdown == ServerOptions.PulsePerSeconds*1)
                    Broadcast("%R%Shutdown in 1%x%");
                else if (_pulseBeforeShutdown == 0)
                {
                    Broadcast("%R%Shutdown NOW!!!%x%");
                    Stop();
                }
            }
        }

        private void HandlePeriodicAuras()
        {
            // TODO: remove aura with amount == 0 ?
            // Remove dot/hot on non-impersonated if source is not the in same room (or source is inexistant)
            // TODO: take periodic aura that will be processed/removed
            //IReadOnlyCollection<ICharacter> clonePeriodicAuras = new ReadOnlyCollection<ICharacter>(Repository.World.Characters().Where(x => x.PeriodicAuras.Any()).ToList());
            //foreach (ICharacter character in clonePeriodicAuras)
            foreach (ICharacter character in Repository.World.Characters.Where(x => x.PeriodicAuras.Any()))
            {
                try
                {
                    IReadOnlyCollection<IPeriodicAura> clonePeriodicAuras = new ReadOnlyCollection<IPeriodicAura>(character.PeriodicAuras.ToList()); // must be cloned because collection may be modified during foreach
                    foreach (IPeriodicAura pa in clonePeriodicAuras)
                    {
                        // On NPC, remove hot/dot from unknown source or source not in the same room
                        if (character.ImpersonatedBy == null && (pa.Source == null || pa.Source.Room != character.Room))
                        {
                            pa.OnVanished();
                            character.RemovePeriodicAura(pa);
                        }
                        else // Otherwise, process normally
                        {
                            if (pa.TicksLeft > 0)
                                pa.Process(character);
                            if (pa.TicksLeft == 0) // no else, because Process decrease PeriodsLeft
                            {
                                pa.OnVanished();
                                character.RemovePeriodicAura(pa);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Exception while handling periodic auras of {0}. Exception: {1}", character.DebugName, ex);
                }
            }
        }

        private void HandleAuras() 
        {
            // TODO: remove aura with amount == 0 ?
            // Take aura that will expired
            //IReadOnlyCollection<ICharacter> cloneAuras = new ReadOnlyCollection<ICharacter>(Repository.World.Characters().Where(x => x.Auras.Any(b => b.SecondsLeft <= 0)).ToList());
            //foreach (ICharacter character in cloneAuras)
            foreach (ICharacter character in Repository.World.Characters.Where(x => x.Auras.Any(b => b.SecondsLeft <= 0)))
            {
                try
                {
                    IReadOnlyCollection<IAura> cloneAuras = new ReadOnlyCollection<IAura>(character.Auras.ToList()); // must be cloned because collection may be modified during foreach
                    bool needsRecompute = false;
                    foreach (IAura aura in cloneAuras.Where(x => x.SecondsLeft <= 0))
                    {
                        aura.OnVanished();
                        character.RemoveAura(aura, false); // recompute once each aura has been processed
                        needsRecompute = true;
                    }
                    if (needsRecompute)
                        character.RecomputeAttributes();
                }
                catch (Exception ex)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Exception while handling auras of {0}. Exception: {1}", character.DebugName, ex);
                }
            }
        }

        private void HandleCooldowns() 
        {
            // TODO: filter on character with expired cooldowns
            foreach (ICharacter character in Repository.World.Characters.Where(x => x.HasAbilitiesInCooldown))
            {
                try
                {
                    IReadOnlyCollection<KeyValuePair<IAbility, DateTime>> cooldowns = new ReadOnlyCollection<KeyValuePair<IAbility, DateTime>>(character.AbilitiesInCooldown.ToList()); // clone
                    foreach (IAbility ability in cooldowns.Where(x => (x.Value - CurrentTime).TotalSeconds <= 0).Select(x => x.Key))
                        character.ResetCooldown(ability, true);
                }
                catch (Exception ex)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Exception while handling cooldowns of {0}. Exception: {1}", character.DebugName, ex);
                }
            }
        }

        // TODO: 'Optimize' following function using area info such as players count

        private void HandleViolence()
        {
            //IReadOnlyCollection<ICharacter> clone = new ReadOnlyCollection<ICharacter>(Repository.World.Characters().Where(x => x.Fighting != null).ToList());
            //foreach (ICharacter character in clone)
            var fighters = Repository.World.Characters.Where(x => x.Fighting != null).ToList();
            foreach (ICharacter character in Repository.World.Characters.Where(x => x.Fighting != null))
            {
                ICharacter victim = character.Fighting;
                if (victim != null)
                {
                    try
                    {
                        if (victim.Room == character.Room) // fight continue only if in the same room
                        {
                            Log.Default.WriteLine(LogLevels.Debug, "Continue fight between {0} and {1}", character.DebugName, victim.DebugName);
                            character.MultiHit(victim);
                        }
                        else
                        {
                            Log.Default.WriteLine(LogLevels.Debug, "Stop fighting between {0} and {1}, because not in same room", character.DebugName, victim.DebugName);
                            character.StopFighting(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Default.WriteLine(LogLevels.Error, "Exception while handling violence of {0}. Exception: {1}", character.DebugName, ex);
                    }
                }
            }
        }

        private void HandlePlayers()
        {
            foreach (PlayingClient playingClient in _players.Values)
            {
                //
                playingClient.Client.WriteData("--TICK--"+Environment.NewLine); // TODO: only if user want tick info

                // If idle for too long, unimpersonate or disconnect
                TimeSpan ts = Repository.Server.CurrentTime - playingClient.LastReceivedDataTimestamp;
                if (ts.TotalMinutes > ServerOptions.IdleMinutesBeforeUnimpersonate && playingClient.Player.Impersonating != null)
                {
                    playingClient.Client.WriteData("Idle for too long, unimpersonating..." + Environment.NewLine);
                    playingClient.Player.Impersonating.StopFighting(true);
                    playingClient.Player.StopImpersonating();
                }
                else if (ts.TotalMinutes > ServerOptions.IdleMinutesBeforeDisconnect)
                {
                    playingClient.Client.WriteData("Idle for too long, disconnecting..." + Environment.NewLine);
                    ClientPlayingOnDisconnected(playingClient.Client);
                }
            }
        }

        private void HandleCharacters()
        {
            foreach (ICharacter character in Repository.World.Characters)
            {
                if (character.Impersonable && character.ImpersonatedBy == null) // TODO: remove after x minutes
                    Log.Default.WriteLine(LogLevels.Warning, $"Impersonable {character.DebugName} is not impersonated");

                //
                character.UpdateResources();
            }
        }

        private void HandleItems()
        {
            foreach (IItem item in Repository.World.Items.Where(x => x.DecayPulseLeft > 0))
            {
                //Log.Default.WriteLine(LogLevels.Debug, $"HandleItems {item.DebugName} with {item.DecayPulseLeft} pulse left");
                item.DecreaseDecayPulseLeft();
                if (item.DecayPulseLeft == 0)
                {
                    Log.Default.WriteLine(LogLevels.Debug, $"Item {item.DebugName} decayed");
                    // TODO: if it's a player corpse, move items to room (except quest item)
                    Repository.World.RemoveItem(item);
                }
            }
        }

        private void HandleRooms()
        {
            // TODO
        }

        private void Cleanup()
        {
            // Remove invalid entities
            Repository.World.Cleanup();
        }

        private void GameLoopTask()
        {
            PulseManager pulseManager = new PulseManager();
            pulseManager.Add(ServerOptions.PulsePerSeconds, ServerOptions.PulseViolence, HandleViolence);
            pulseManager.Add(ServerOptions.PulsePerSeconds, ServerOptions.PulsePerSeconds, HandlePeriodicAuras);
            pulseManager.Add(ServerOptions.PulsePerSeconds, ServerOptions.PulsePerSeconds, HandleAuras);
            pulseManager.Add(ServerOptions.PulsePerSeconds, ServerOptions.PulsePerSeconds, HandleCooldowns);
            pulseManager.Add(ServerOptions.PulsePerSeconds, ServerOptions.PulsePerSeconds*60, HandlePlayers);
            pulseManager.Add(ServerOptions.PulsePerSeconds, ServerOptions.PulsePerSeconds*60, HandleCharacters);
            pulseManager.Add(ServerOptions.PulsePerSeconds, ServerOptions.PulsePerSeconds*60, HandleItems);
            pulseManager.Add(ServerOptions.PulsePerSeconds, ServerOptions.PulsePerSeconds*60, HandleRooms);

            try
            {
                Stopwatch sw = new Stopwatch();
                while (true)
                {
                    sw.Restart();
                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        Log.Default.WriteLine(LogLevels.Info, "Stop GameLoopTask requested");
                        break;
                    }

                    CurrentTime = DateTime.Now;

                    ProcessInput();

                    //DoPulse();
                    HandleShutdown();
                    pulseManager.Pulse();

                    ProcessOutput();

                    Cleanup();

                    sw.Stop();
                    long elapsedMs = sw.ElapsedMilliseconds; // in milliseconds
                    if (elapsedMs < ServerOptions.PulseDelay)
                    {
                        //long elapsedTick = sw.ElapsedTicks; // 1 tick = 1 second/Stopwatch.Frequency
                        //long elapsedNs = sw.Elapsed.Ticks; // 1 tick = 1 nanosecond
                        //Log.Default.WriteLine(LogLevels.Debug, "Elapsed {0}Ms {1}Ticks {2}Ns", elapsedMs, elapsedTick, elapsedNs);
                        //Thread.Sleep(250 - (int) elapsedMs);
                        int sleepTime = ServerOptions.PulseDelay - (int) elapsedMs;
                        _cancellationTokenSource.Token.WaitHandle.WaitOne(sleepTime);
                    }
                    else
                    {
                        Log.Default.WriteLine(LogLevels.Error, "!!! No sleep for GameLoopTask. Elapsed {0}", elapsedMs);
                        _cancellationTokenSource.Token.WaitHandle.WaitOne(1);
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                Log.Default.WriteLine(LogLevels.Error, "GameLoopTask exception. Exception: {0}", ex);
            }

            pulseManager.Clear();

            Log.Default.WriteLine(LogLevels.Info, "GameLoopTask stopped");
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposeOfManagedResourcesInAdditionToUnmanagedResources)
        {
            if (disposeOfManagedResourcesInAdditionToUnmanagedResources)
            {
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                }
            }
        }

        #endregion
    }
}
