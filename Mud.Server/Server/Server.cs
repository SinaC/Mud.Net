using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mud.Container;
using Mud.Repository;
using Mud.Domain;
using Mud.Logger;
using Mud.Network;
using Mud.Server.Abilities;
using Mud.Server.Blueprints.Item;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Settings;
using System.Reflection;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Item;

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
    public class Server : IServer, IWiznet, IPlayerManager, IAdminManager, IServerAdminCommand, IServerPlayerCommand, IDisposable
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

        protected ISettings Settings => DependencyContainer.Current.GetInstance<ISettings>();
        protected IWorld World => DependencyContainer.Current.GetInstance<IWorld>();
        protected IClassManager ClassManager => DependencyContainer.Current.GetInstance<IClassManager>();
        protected IRaceManager RaceManager => DependencyContainer.Current.GetInstance<IRaceManager>();
        protected IAbilityManager AbilityManager => DependencyContainer.Current.GetInstance<IAbilityManager>();
        protected ILoginRepository LoginRepository => DependencyContainer.Current.GetInstance<ILoginRepository>();
        protected IPlayerRepository PlayerRepository => DependencyContainer.Current.GetInstance<IPlayerRepository>();
        protected IUniquenessManager UniquenessManager => DependencyContainer.Current.GetInstance<IUniquenessManager>();
        protected ITimeManager TimeManager => DependencyContainer.Current.GetInstance<ITimeManager>();

        public Server()
        {
            _clients = new ConcurrentDictionary<IClient, PlayingClient>();
            _players = new ConcurrentDictionary<IPlayer, PlayingClient>();
            _loginInClients = new ConcurrentDictionary<IClient, LoginStateMachine>();
        }

        #region IServer

        public void Initialize(List<INetworkServer> networkServers)
        {
            _networkServers = networkServers;
            foreach (INetworkServer networkServer in _networkServers)
            {
                networkServer.NewClientConnected += NetworkServerOnNewClientConnected;
                networkServer.ClientDisconnected += NetworkServerOnClientDisconnected;
            }

            TimeManager.Initialize();

            // Perform some validity/sanity checks
            if (Settings.PerformSanityCheck)
                SanityChecks();

            // Dump config
            if (Settings.DumpOnInitialize)
                Dump();

            // TODO: other sanity checks
            // TODO: check room/item/character id uniqueness

            // Initialize UniquenessManager
            UniquenessManager.Initialize();
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

        public void Dump()
        {
            DumpCommands();
            DumpClasses();
            DumpRaces();
            DumpClasses();
            DumpAbilities();
        }

        #endregion

        #region IWiznet

        public void Wiznet(string message, WiznetFlags flags, AdminLevels minLevel = AdminLevels.Angel)
        {
            foreach (IAdmin admin in Admins.Where(a => a.WiznetFlags.HasFlag(flags) && a.Level >= minLevel))
                admin.Send($"%W%WIZNET%x%:{message}");
        }

        #endregion

        #region IPlayerManager

        public IPlayer GetPlayer(CommandParameter parameter, bool perfectMatch) => FindHelpers.FindByName(_players.Keys, parameter, perfectMatch);

        public IEnumerable<IPlayer> Players => _players.Keys;

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

        #endregion

        #region IAdminManager

        public IAdmin GetAdmin(CommandParameter parameter, bool perfectMatch) => FindHelpers.FindByName(_players.Keys.OfType<IAdmin>(), parameter, perfectMatch);

        public IEnumerable<IAdmin> Admins => _players.Keys.OfType<IAdmin>();

        // TODO: remove
        // TEST PURPOSE
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

        #region IServerAdminCommand

        public void Shutdown(int seconds)
        {
            int minutes = seconds / 60;
            int remaining = seconds % 60;
            if (minutes > 0 && remaining != 0)
                Broadcast($"%R%Shutdown in {minutes} minute{(minutes > 1 ? "s" : string.Empty)} and {remaining} second{(remaining > 1 ? "s" : string.Empty)}%x%");
            else if (minutes > 0 && remaining == 0)
                Broadcast($"%R%Shutdown in {minutes} minute{(minutes > 1 ? "s" : string.Empty)}%x%");
            else
                Broadcast($"%R%Shutdown in {seconds} second{(seconds > 1 ? "s" : string.Empty)}%x%");
            _pulseBeforeShutdown = seconds * Pulse.PulsePerSeconds;
        }

        public void Promote(IPlayer player, AdminLevels level)
        {
            // TODO: should be done atomically
            if (player is IAdmin)
            {
                Log.Default.WriteLine(LogLevels.Error, "Promote: client is already admin");
                return;
            }
            PlayingClient playingClient;
            _players.TryGetValue(player, out playingClient);
            if (playingClient == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Promote: client not found");
                return;
            }

            // Let's go
            Log.Default.WriteLine(LogLevels.Info, "Promoting {0} to {1}", player.Name, level);
            Wiznet($"Promoting {player.Name} to {level}", WiznetFlags.Promote);

            // Remove from playing client
            lock (_playingClientLockObject)
            {
                _clients.TryRemove(playingClient.Client, out playingClient);
                _players.TryRemove(playingClient.Player, out playingClient);
                // !!! PlayingClient removed from both collection must be equal
            }

            // Unlink SendData and PageData
            player.SendData -= PlayerOnSendData;
            player.PageData -= PlayerOnPageData;

            // Reset LastTeller and SnoopBy
            player.SetLastTeller(null);
            player.SetSnoopBy(null);

            // Stop impersonation if any + stop fights
            if (player.Impersonating != null)
            {
                player.Impersonating.StopFighting(true);
                player.StopImpersonating();
            }

            // Create admin
            IAdmin admin = new Admin.Admin(player.Id, player.Name, level, player.Aliases, player.Avatars);

            // Replace player by admin in playingClient
            playingClient.Player = admin;

            // Link SendData and PageData
            admin.SendData += PlayerOnSendData;
            admin.PageData += PlayerOnPageData;

            // Reinsert in playing client
            lock (_playingClientLockObject)
            {
                _players.TryAdd(admin, playingClient);
                _clients.TryAdd(playingClient.Client, playingClient);
            }

            // Delete player
            PlayerRepository.Delete(player.Name);

            // Save admin
            admin.Save();

            // Save login
            LoginRepository.ChangeAdminStatus(admin.Name, true);

            // Inform admin about promotion
            admin.Send("You have been promoted to {0}", level);
        }

        #endregion

        #region IServerPlayerCommand

        public void Quit(IPlayer player)
        {
            PlayingClient playingClient;
            _players.TryGetValue(player, out playingClient);
            if (playingClient == null)
                Log.Default.WriteLine(LogLevels.Error, "Quit: client not found");
            else
                ClientPlayingOnDisconnected(playingClient.Client);
        }

        public void Delete(IPlayer player)
        {
            PlayingClient playingClient;
            _players.TryGetValue(player, out playingClient);
            if (playingClient == null)
                Log.Default.WriteLine(LogLevels.Error, "Delete: client not found");
            else
            {
                string playerName = player.DisplayName;
                LoginRepository.DeleteLogin(player.Name);
                PlayerRepository.Delete(player.Name);
                UniquenessManager.RemoveAvatarNames(player.Avatars?.Select(x => x.Name));
                ClientPlayingOnDisconnected(playingClient.Client);
                //
                Log.Default.WriteLine(LogLevels.Info, "Player {0} has been deleted", playerName);
            }
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
            if (isNewPlayer)
            {
            }

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

            // Save if isNewPlayer
            if (isNewPlayer)
                playerOrAdmin.Save();

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
                HandlePaging(playingClient, string.Empty);
            }
        }

        #endregion

        // Once paging is active, classic commands are not processed anymore
        // Valid commands are (Enter), (N)ext, (P)revious, (Q)uit, (A)ll
        private void HandlePaging(PlayingClient playingClient, string command)
        {
            string lowerCommand = command.ToLowerInvariant();
            if (command == string.Empty || "next".StartsWith(lowerCommand)) // <Enter> or Next -> send next page
            {
                // Pages are always sent immediately asynchronously, don't use ProcessOutput even if in synchronous mode
                string nextPage = playingClient.Paging.GetNextPage(playingClient.Player.PagingLineCount);
                playingClient.Client.WriteData(nextPage);
                if (playingClient.Paging.HasPageLeft) // page left, send page instructions (no prompt)
                    playingClient.Client.WriteData(StringHelpers.PagingInstructions);
                else // no more page -> normal mode
                {
                    playingClient.Paging.Clear();
                    playingClient.Client.WriteData(playingClient.Player.Prompt);
                }
            }
            else if ("previous".StartsWith(lowerCommand))
            {
                string previousPage = playingClient.Paging.GetPreviousPage(playingClient.Player.PagingLineCount);
                playingClient.Client.WriteData(previousPage);
                if (playingClient.Paging.HasPageLeft) // page left, send page instructions (no prompt)
                    playingClient.Client.WriteData(StringHelpers.PagingInstructions);
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
            else
                playingClient.Client.WriteData(StringHelpers.PagingInstructions);
        }

        private void Broadcast(string message)
        {
            message += Environment.NewLine;
            // By-pass process output
            lock (_playingClientLockObject)
                foreach (IClient client in _clients.Keys)
                    client.WriteData(message);
        }

        private void ProcessInput()
        {
            // Read one command from each client and process it
            foreach (PlayingClient playingClient in _players.Values.Shuffle()) // !! players list cannot be modified while processing inputs
            {
                if (playingClient.Player != null)
                {
                    string command = string.Empty;
                    try
                    {
                        if (playingClient.Player.GlobalCooldown > 0) // if player is on GCD, decrease it
                            playingClient.Player.DecreaseGlobalCooldown();
                        else
                        {
                            command = playingClient.DequeueReceivedData(); // process one command at a time
                            if (command != null)
                            {
                                if (playingClient.Paging.HasPageLeft) // if paging, valid commands are <Enter>, Next, Quit, All
                                    HandlePaging(playingClient, command);
                                else if (!string.IsNullOrWhiteSpace(command))
                                {
                                    playingClient.Player.ProcessCommand(command); // TODO: if command takes time to be processed, 'next' players will be delayed
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Default.WriteLine(LogLevels.Error, "Exception while processing input of {0} [{1}]. Exception: {2}", playingClient.Player.Name, command, ex);
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
                        if (!string.IsNullOrWhiteSpace(datas))
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

        private void SanityChecks()
        {
            SanityCheckQuests();
            SanityCheckAbilities();
            SanityCheckClasses();
            SanityCheckRaces();
            SanityCheckRooms();
            SanityCheckItems();
            SanityCheckCharacters();
        }

        private void SanityCheckAbilities()
        {
            var duplicateIds = AbilityManager.Abilities.GroupBy(x => x.Id).Where(x => x.Count() > 1);
            foreach(var duplicate in duplicateIds)
                Log.Default.WriteLine(LogLevels.Error, "Ability duplicate id {0}: {1}", duplicate.Key, string.Join(",", duplicate.Select(x => x.Name)));

            var duplicateNames = AbilityManager.Abilities.GroupBy(x => x.Name).Where(x => x.Count() > 1);
            foreach (var duplicate in duplicateNames)
                Log.Default.WriteLine(LogLevels.Error, "Ability duplicate name {0}: {1}", duplicate.Key, string.Join(",", duplicate.Select(x => x.Id.ToString())));

            foreach (var ability in AbilityManager.Abilities)
            {
                if (!string.IsNullOrWhiteSpace(ability.DispelRoomMessage) && !ability.AbilityFlags.HasFlag(AbilityFlags.CanBeDispelled))
                    Log.Default.WriteLine(LogLevels.Warning, "Ability {0} has dispel message but is not flagged as CanBeDispelled", ability.Name);
            }

            Log.Default.WriteLine(LogLevels.Info, "#Abilities: {0}", AbilityManager.Abilities.Count());
            Log.Default.WriteLine(LogLevels.Info, "#Passives: {0}", AbilityManager.Passives.Count());
            Log.Default.WriteLine(LogLevels.Info, "#Spells: {0}", AbilityManager.Spells.Count());
            Log.Default.WriteLine(LogLevels.Info, "#Skills: {0}", AbilityManager.Skills.Count());
        }

        private void SanityCheckClasses()
        {
            foreach (IClass c in ClassManager.Classes)
            {
                if (c.MaxHitPointGainPerLevel < c.MinHitPointGainPerLevel)
                    Log.Default.WriteLine(LogLevels.Warning, "Class {0} max hp per level < min hp per level");
                if (c.ResourceKinds == null || !c.ResourceKinds.Any())
                    Log.Default.WriteLine(LogLevels.Warning, "Class {0} doesn't have any allowed resources", c.Name);
                else
                {
                    foreach (AbilityUsage abilityUsage in c.Abilities)
                        if (abilityUsage.ResourceKind.HasValue && !c.ResourceKinds.Contains(abilityUsage.ResourceKind.Value))
                            Log.Default.WriteLine(LogLevels.Warning, "Class {0} is allowed to use ability {1} [resource:{2}] but doesn't have access to that resource", c.DisplayName, abilityUsage.Ability.Name, abilityUsage.ResourceKind);
                }
            }
            Log.Default.WriteLine(LogLevels.Info, "#Classes: {0}", ClassManager.Classes.Count());
        }

        private void SanityCheckRaces()
        {
            Log.Default.WriteLine(LogLevels.Info, "#Races: {0}", RaceManager.Races.Count());
        }

        private void SanityCheckQuests()
        {
            foreach (QuestBlueprint questBlueprint in World.QuestBlueprints)
            {
                if (questBlueprint.ItemObjectives?.Length == 0 && questBlueprint.KillObjectives?.Length == 0 && questBlueprint.LocationObjectives?.Length == 0)
                    Log.Default.WriteLine(LogLevels.Error, "Quest id {0} doesn't have any objectives.", questBlueprint.Id);
                else
                {
                    var duplicateIds = (questBlueprint.ItemObjectives ?? Enumerable.Empty<QuestItemObjectiveBlueprint>()).Select(x => x.Id).Union((questBlueprint.KillObjectives ?? Enumerable.Empty<QuestKillObjectiveBlueprint>()).Select(x => x.Id)).Union((questBlueprint.LocationObjectives ?? Enumerable.Empty<QuestLocationObjectiveBlueprint>()).Select(x => x.Id))
                        .GroupBy(x => x, (id, ids) => new { objectiveId = id, count = ids.Count() }).Where(x => x.count > 1);
                    foreach (var duplicateId in duplicateIds)
                        Log.Default.WriteLine(LogLevels.Error, "Quest id {0} has objectives with duplicate id {1} count {2}", questBlueprint.Id, duplicateId.objectiveId, duplicateId.count);
                }
            }
            Log.Default.WriteLine(LogLevels.Info, "#QuestBlueprints: {0}", World.QuestBlueprints.Count);
        }

        private void SanityCheckRooms()
        {
            Log.Default.WriteLine(LogLevels.Info, "#RoomBlueprints: {0}", World.RoomBlueprints.Count);
            Log.Default.WriteLine(LogLevels.Info, "#Rooms: {0}", World.Rooms.Count());
        }

        private void SanityCheckItems()
        {
            Log.Default.WriteLine(LogLevels.Info, "#ItemBlueprints: {0}", World.ItemBlueprints.Count);
            Log.Default.WriteLine(LogLevels.Info, "#Items: {0}", World.Items.Count());
            if (World.GetItemBlueprint<ItemCorpseBlueprint>(Settings.CorpseBlueprintId) == null)
                Log.Default.WriteLine(LogLevels.Error, "Item corpse blueprint {0} not found or not a corpse", Settings.CorpseBlueprintId);
            if (World.GetItemBlueprint<ItemFoodBlueprint>(Settings.MushroomBlueprintId) == null)
                Log.Default.WriteLine(LogLevels.Error, "'a Magic mushroom' blueprint {0} not found or not food (needed for spell CreateFood)", Settings.MushroomBlueprintId);
            if (World.GetItemBlueprint<ItemFountainBlueprint>(Settings.SpringBlueprintId) == null)
                Log.Default.WriteLine(LogLevels.Error, "'a magical spring' blueprint {0} not found or not a fountain (needed for spell CreateSpring)", Settings.SpringBlueprintId);
            if (World.GetItemBlueprint<ItemLightBlueprint>(Settings.LightBallBlueprintId) == null)
                Log.Default.WriteLine(LogLevels.Error, "'a bright ball of light' blueprint {0} not found or not an light (needed for spell ContinualLight)", Settings.LightBallBlueprintId);
        }

        private void SanityCheckCharacters()
        {
            Log.Default.WriteLine(LogLevels.Info, "#CharacterBlueprints: {0}", World.CharacterBlueprints.Count);
            Log.Default.WriteLine(LogLevels.Info, "#Characters: {0}", World.Characters.Count());
        }

        private void DumpCommands()
        {
            //DumpCommandByType(typeof(Admin.Admin));
            //DumpCommandByType(typeof(Player.Player));
            //DumpCommandByType(typeof(NonPlayableCharacter));
            //DumpCommandByType(typeof(PlayableCharacter));
            //DumpCommandByType(typeof(Item.ItemBase<>));
            //DumpCommandByType(typeof(Room.Room));
            Type actorBaseType = typeof(Actor.ActorBase);
            var actorTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsClass && !x.IsAbstract && actorBaseType.IsAssignableFrom(x))
                .ToList();
            foreach (Type actorType in actorTypes)
                DumpCommandByType(actorType);
        }

        private void DumpCommandByType(Type t)
        {
            for (char c = 'a'; c <= 'z'; c++)
            {
                CommandMethodInfo[] query = CommandHelpers.GetCommands(t).GetByPrefix(c.ToString()).OrderBy(x => x.Value.Attribute.Priority).Select(x => x.Value).ToArray();

                if (query.Length == 0)
                    Log.Default.WriteLine(LogLevels.Debug, $"No commands for {t.Name} prefix '{c}'"); // Dump in log
                else
                {
                    StringBuilder sb = TableGenerators.CommandMethodInfoTableGenerator.Value.Generate($"Commands for {t.Name} prefix '{c}'", query);
                    Log.Default.WriteLine(LogLevels.Debug, sb.ToString()); // Dump in log
                }
            }
        }

        private void DumpClasses()
        {
            StringBuilder sb = TableGenerators.ClassTableGenerator.Value.Generate("Classes", ClassManager.Classes.OrderBy(x => x.Name));
            Log.Default.WriteLine(LogLevels.Debug, sb.ToString()); // Dump in log
        }

        private void DumpRaces()
        {
            StringBuilder sb = TableGenerators.RaceTableGenerator.Value.Generate("Races", RaceManager.Races.OrderBy(x => x.Name));
            Log.Default.WriteLine(LogLevels.Debug, sb.ToString()); // Dump in log
        }

        private void DumpAbilities()
        {
            StringBuilder sb = TableGenerators.FullInfoAbilityTableGenerator.Value.Generate("Abilities", AbilityManager.Abilities.OrderBy(x => x.Name));
            Log.Default.WriteLine(LogLevels.Debug, sb.ToString()); // Dump in log
        }

        private void HandleShutdown(int _)
        {
            if (_pulseBeforeShutdown >= 0)
            {
                _pulseBeforeShutdown--;
                if (_pulseBeforeShutdown == Pulse.PulsePerMinutes*15)
                    Broadcast("%R%Shutdown in 15 minutes%x%");
                if (_pulseBeforeShutdown == Pulse.PulsePerMinutes*10)
                    Broadcast("%R%Shutdown in 10 minutes%x%");
                if (_pulseBeforeShutdown == Pulse.PulsePerMinutes*5)
                    Broadcast("%R%Shutdown in 5 minutes%x%");
                if (_pulseBeforeShutdown == Pulse.PulsePerMinutes)
                    Broadcast("%R%Shutdown in 1 minute%x%");
                if (_pulseBeforeShutdown == Pulse.PulsePerSeconds*30)
                    Broadcast("%R%Shutdown in 30 seconds%x%");
                if (_pulseBeforeShutdown == Pulse.PulsePerSeconds*15)
                    Broadcast("%R%Shutdown in 15 seconds%x%");
                if (_pulseBeforeShutdown == Pulse.PulsePerSeconds*10)
                    Broadcast("%R%Shutdown in 10 seconds%x%");
                if (_pulseBeforeShutdown == Pulse.PulsePerSeconds*5)
                    Broadcast("%R%Shutdown in 5%x%");
                if (_pulseBeforeShutdown == Pulse.PulsePerSeconds*4)
                    Broadcast("%R%Shutdown in 4%x%");
                if (_pulseBeforeShutdown == Pulse.PulsePerSeconds*3)
                    Broadcast("%R%Shutdown in 3%x%");
                if (_pulseBeforeShutdown == Pulse.PulsePerSeconds*2)
                    Broadcast("%R%Shutdown in 2%x%");
                if (_pulseBeforeShutdown == Pulse.PulsePerSeconds*1)
                    Broadcast("%R%Shutdown in 1%x%");
                else if (_pulseBeforeShutdown == 0)
                {
                    Broadcast("%R%Shutdown NOW!!!%x%");
                    Stop();
                }
            }
        }

        private void HandlePeriodicAuras(int pulseCount)
        {
            // TODO: remove aura with amount == 0 ?
            // Remove dot/hot on non-impersonated if source is not the in same room (or source is inexistant)
            // TODO: take periodic aura that will be processed/removed
            //IReadOnlyCollection<ICharacter> clonePeriodicAuras = new ReadOnlyCollection<ICharacter>(World.Characters().Where(x => x.PeriodicAuras.Any()).ToList());
            //foreach (ICharacter character in clonePeriodicAuras)
            foreach (ICharacter character in World.Characters.Where(x => x.PeriodicAuras.Any()))
            {
                try
                {
                    IReadOnlyCollection<IPeriodicAura> clonePeriodicAuras = new ReadOnlyCollection<IPeriodicAura>(character.PeriodicAuras.ToList()); // must be cloned because collection may be modified during foreach
                    foreach (IPeriodicAura pa in clonePeriodicAuras)
                    {
                        // On NPC, remove hot/dot from unknown source or source not in the same room
                        ICharacter sourceAsCharacter = pa.Source as ICharacter;
                        if (Settings.RemovePeriodicAurasInNotInSameRoom && character is INonPlayableCharacter && (pa.Source == null || sourceAsCharacter == null || sourceAsCharacter.Room != character.Room))
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

        private void HandleAuras(int pulseCount) 
        {
            foreach (ICharacter character in World.Characters.Where(x => x.Auras.Any(b => b.PulseLeft > 0)))
            {
                try
                {
                    bool needsRecompute = false;
                    IReadOnlyCollection<IAura> cloneAuras = new ReadOnlyCollection<IAura>(character.Auras.ToList()); // must be cloned because collection may be modified during foreach
                    foreach (IAura aura in cloneAuras.Where(x => x.PulseLeft > 0))
                    {
                        bool timedOut = aura.DecreasePulseLeft(pulseCount);
                        if (timedOut)
                        {
                            //TODO: aura.OnVanished();
                            // TODO: Set Validity to false
                            character.RemoveAura(aura, false); // recompute once each aura has been processed
                            needsRecompute = true;
                        }
                    }
                    if (needsRecompute)
                        character.Recompute();
                    // TODO: remove invalid auras
                }
                catch (Exception ex)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Exception while handling auras of character {0}. Exception: {1}", character.DebugName, ex);
                }
            }
            foreach (IItem item in World.Items.Where(x => x.Auras.Any(b => b.PulseLeft > 0)))
            {
                try
                {
                    bool needsRecompute = false;
                    IReadOnlyCollection<IAura> cloneAuras = new ReadOnlyCollection<IAura>(item.Auras.ToList()); // must be cloned because collection may be modified during foreach
                    foreach (IAura aura in cloneAuras.Where(x => x.PulseLeft > 0))
                    {
                        bool timedOut = aura.DecreasePulseLeft(pulseCount);
                        if (timedOut)
                        {
                            //TODO: aura.OnVanished();
                            // TODO: Set Validity to false
                            item.RemoveAura(aura, false); // recompute once each aura has been processed
                            needsRecompute = true;
                        }
                    }
                    if (needsRecompute)
                        item.Recompute();
                    // TODO: remove invalid auras
                }
                catch (Exception ex)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Exception while handling auras of item {0}. Exception: {1}", item.DebugName, ex);
                }
            }
            foreach (IRoom room in World.Rooms.Where(x => x.Auras.Any(b => b.PulseLeft > 0)))
            {
                try
                {
                    bool needsRecompute = false;
                    IReadOnlyCollection<IAura> cloneAuras = new ReadOnlyCollection<IAura>(room.Auras.ToList()); // must be cloned because collection may be modified during foreach
                    foreach (IAura aura in cloneAuras.Where(x => x.PulseLeft > 0))
                    {
                        bool timedOut = aura.DecreasePulseLeft(pulseCount);
                        if (timedOut)
                        {
                            //TODO: aura.OnVanished();
                            // TODO: Set Validity to false
                            room.RemoveAura(aura, false); // recompute once each aura has been processed
                            needsRecompute = true;
                        }
                    }
                    if (needsRecompute)
                        room.Recompute();
                    // TODO: remove invalid auras
                }
                catch (Exception ex)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Exception while handling auras of room {0}. Exception: {1}", room.DebugName, ex);
                }
            }
        }

        private void HandleCooldowns(int pulseCount) 
        {
            // TODO: filter on character with expired cooldowns
            foreach (ICharacter character in World.Characters.Where(x => x.HasAbilitiesInCooldown))
            {
                try
                {
                    IReadOnlyCollection<KeyValuePair<IAbility, DateTime>> cooldowns = new ReadOnlyCollection<KeyValuePair<IAbility, DateTime>>(character.AbilitiesInCooldown.ToList()); // clone
                    foreach (IAbility ability in cooldowns.Where(x => (x.Value - TimeManager.CurrentTime).TotalSeconds <= 0).Select(x => x.Key))
                        character.ResetCooldown(ability, true);
                }
                catch (Exception ex)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Exception while handling cooldowns of {0}. Exception: {1}", character.DebugName, ex);
                }
            }
        }

        private void HandleQuests(int pulseCount)
        {
            foreach (IPlayer player in Players.Where(x => x.Impersonating?.Quests?.Any(y => y.Blueprint.TimeLimit > 0) == true))
            {
                try
                {
                    IReadOnlyCollection<IQuest> clone = new ReadOnlyCollection<IQuest>(player.Impersonating.Quests.Where(x => x.Blueprint.TimeLimit > 0).ToList());
                    foreach (IQuest quest in clone)
                    {
                        bool timedOut = quest.DecreasePulseLeft(pulseCount);
                        if (timedOut)
                        {
                            quest.Timeout();
                            player.Impersonating.RemoveQuest(quest);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Exception while handling cooldowns of {0}. Exception: {1}", player.Impersonating.DebugName, ex);
                }
            }
        }

        // TODO: 'Optimize' following function using area info such as players count

        private void HandleViolence(int pulseCount)
        {
            //Log.Default.WriteLine(LogLevels.Debug, "HandleViolence: {0}", DateTime.Now);
            foreach (ICharacter character in World.Characters.Where(x => x.Fighting != null))
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
                            if (character is INonPlayableCharacter nonPlayableCharacter)
                            {
                                Log.Default.WriteLine(LogLevels.Debug, "Non-playable character stop fighting, resetting it");
                                nonPlayableCharacter.Reset();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Default.WriteLine(LogLevels.Error, "Exception while handling violence {0}. Exception: {1}", character.DebugName, ex);
                    }
                }
            }
        }

        private void HandlePlayers(int pulseCount)
        {
            foreach (PlayingClient playingClient in _players.Values)
            {
                try
                {
                    //
                    playingClient.Client.WriteData("--TICK--" + Environment.NewLine); // TODO: only if user want tick info
                    string prompt = playingClient.Player.Prompt;
                    playingClient.Client.WriteData(prompt); // display prompt at each tick

                    // If idle for too long, unimpersonate or disconnect
                    TimeSpan ts = TimeManager.CurrentTime - playingClient.LastReceivedDataTimestamp;
                    if (ts.TotalMinutes > Settings.IdleMinutesBeforeUnimpersonate && playingClient.Player.Impersonating != null)
                    {
                        playingClient.Client.WriteData("Idle for too long, unimpersonating..." + Environment.NewLine);
                        playingClient.Player.Impersonating.StopFighting(true);
                        playingClient.Player.StopImpersonating();
                    }
                    else if (ts.TotalMinutes > Settings.IdleMinutesBeforeDisconnect)
                    {
                        playingClient.Client.WriteData("Idle for too long, disconnecting..." + Environment.NewLine);
                        ClientPlayingOnDisconnected(playingClient.Client);
                    }
                }
                catch (Exception ex)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Exception while handling player {0}. Exception: {1}", playingClient.Player.Name, ex);
                }
            }
        }

        private void HandlePlayableCharacters(int pulseCount)
        {
            foreach (IPlayableCharacter character in World.PlayableCharacters)
            {
                try
                {
                    if (character.ImpersonatedBy == null) // TODO: remove after x minutes
                        Log.Default.WriteLine(LogLevels.Warning, "Impersonable {0} is not impersonated", character.DebugName);

                    // Update resources
                    character.Regen();
                    // Update conditions
                    character.GainCondition(Conditions.Drunk, -1); // decrease drunk state
                    // TODO: not if undead from here
                    character.GainCondition(Conditions.Full, character.Size > Sizes.Medium ? -4 : -2);
                    character.GainCondition(Conditions.Thirst, -1);
                    character.GainCondition(Conditions.Hunger, character.Size > Sizes.Medium ? -2 : -1);
                }
                catch (Exception ex)
                { 
                    Log.Default.WriteLine(LogLevels.Error, "Exception while handling pc character {0}. Exception: {1}", character.DebugName, ex);
                }
            }
        }

        private void HandleNonPlayableCharacters(int pulseCount)
        {
            foreach (INonPlayableCharacter character in World.NonPlayableCharacters)
            {
                try
                {
                    //
                    character.Regen();
                }
                catch (Exception ex)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Exception while handling npc character {0}. Exception: {1}", character.DebugName, ex);
                }
            }
        }

        private void HandleItems(int pulseCount)
        {
            //Log.Default.WriteLine(LogLevels.Debug, "HandleItems {0} {1}", CurrentTime, DateTime.Now);
            foreach (IItem item in World.Items.Where(x => x.DecayPulseLeft > 0))
            {
                try
                {
                    //Log.Default.WriteLine(LogLevels.Debug, $"HandleItems {item.DebugName} with {item.DecayPulseLeft} pulse left");
                    item.DecreaseDecayPulseLeft(pulseCount);
                    if (item.DecayPulseLeft == 0)
                    {
                        Log.Default.WriteLine(LogLevels.Debug, "Item {0} decays", item.DebugName);
                        string msg = "{0:N} crumbles into dust.";
                        switch (item)
                        {
                            case IItemCorpse _:
                                msg = "{0:N} decays into dust.";
                                break;
                            case IItemFountain _:
                                msg = "{0:N} dries up.";
                                break;
                            case IItemFood _:
                                msg = "{0:N} decomposes.";
                                break;
                            // TODO: potion  "$p has evaporated from disuse."
                            case IItemPortal _:
                                msg = "{0:N} fades out of existence.";
                                break;
                            case IItemContainer caseContainer:
                                if (item.WearLocation == WearLocations.Float)
                                {
                                    if (caseContainer.Content.Any())
                                        msg = "{0:N} flickers and vanishes, spilling its contents on the floor.";
                                    else
                                        msg = "{0:N} flickers and vanishes.";
                                }
                                break;
                        }
                        // Display message to character or room
                        if (item.ContainedInto is ICharacter wasOnCharacter)
                            wasOnCharacter.Act(ActOptions.ToCharacter, msg, item);
                        else if (item.ContainedInto is IRoom wasInRoom)
                        {
                            foreach (ICharacter character in wasInRoom.People)
                                character.Act(ActOptions.ToCharacter, msg, item);
                        }

                        // If container or playable character corpse, move items to contained into (except quest item)
                        if (item is IContainer container) // container
                        {
                            bool moveItems = !(item is IItemCorpse itemCorpse && !itemCorpse.IsPlayableCharacterCorpse); // don't perform if NPC corpse
                            if (moveItems)
                            {
                                Log.Default.WriteLine(LogLevels.Debug, "Move item content to room");
                                IContainer newContainer = item.ContainedInto;
                                if (newContainer == null)
                                    Log.Default.WriteLine(LogLevels.Error, "Item was in the void");
                                else
                                {
                                    IReadOnlyCollection<IItem> clone = new ReadOnlyCollection<IItem>(container.Content.Where(x => !(x is IItemQuest)).ToList()); // except quest item
                                    foreach (IItem itemInCorpse in clone)
                                        itemInCorpse.ChangeContainer(newContainer);
                                }
                            }
                        }
                        World.RemoveItem(item);
                    }
                }
                catch (Exception ex)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Exception while handling item {0}. Exception: {1}", item.DebugName, ex);
                }
            }
        }

        private void HandleRooms(int pulseCount)
        {
            // TODO
        }

        private void HandleTime(int pulseCount)
        {
            string timeUpdate = TimeManager.Update();
            Log.Default.WriteLine(LogLevels.Debug, "HandleTime: {0}", !string.IsNullOrWhiteSpace(timeUpdate) ? timeUpdate : "no update");
            if (!string.IsNullOrWhiteSpace(timeUpdate))
            {
                // inform non-sleeping and outdoors players
                foreach (IPlayableCharacter character in World.PlayableCharacters.Where(x => x.Position > Positions.Sleeping && x.Room != null && !x.Room.RoomFlags.HasFlag(RoomFlags.Indoors)))
                {
                    try
                    {
                        character.Send(timeUpdate);
                    }
                    catch (Exception ex)
                    {
                        Log.Default.WriteLine(LogLevels.Error, "Exception while handling time character {0}. Exception: {1}", character.DebugName, ex);
                    }
                }
            }
        }

        private void Cleanup()
        {
            // Remove invalid entities
            World.Cleanup();
        }

        private void GameLoopTask()
        {
            PulseManager pulseManager = new PulseManager();
            pulseManager.Add(Pulse.PulsePerSeconds, Pulse.PulseViolence, HandleViolence);
            pulseManager.Add(Pulse.PulsePerSeconds, Pulse.PulsePerSeconds, HandlePeriodicAuras);
            pulseManager.Add(Pulse.PulsePerSeconds, Pulse.PulsePerSeconds, HandleAuras);
            pulseManager.Add(Pulse.PulsePerSeconds, Pulse.PulsePerSeconds, HandleCooldowns);
            pulseManager.Add(Pulse.PulsePerSeconds, Pulse.PulsePerSeconds, HandleQuests);
            pulseManager.Add(Pulse.PulsePerSeconds, Pulse.PulsePerSeconds * 5, HandleItems);
            pulseManager.Add(Pulse.PulsePerSeconds, Pulse.PulsePerSeconds * 60, HandlePlayableCharacters);
            pulseManager.Add(Pulse.PulsePerSeconds, Pulse.PulsePerSeconds * 60, HandleNonPlayableCharacters);
            pulseManager.Add(Pulse.PulsePerSeconds, Pulse.PulsePerSeconds * 60, HandlePlayers);
            pulseManager.Add(Pulse.PulsePerSeconds, Pulse.PulsePerSeconds * 60, HandleRooms);
            pulseManager.Add(Pulse.PulsePerSeconds, Pulse.PulsePerSeconds * 60, HandleTime); // 1 minute in real life is one hour in game

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

                    TimeManager.FixCurrentTime();

                    ProcessInput();

                    //DoPulse();
                    HandleShutdown(_:1);
                    pulseManager.Pulse();

                    ProcessOutput();

                    Cleanup();

                    sw.Stop();
                    long elapsedMs = sw.ElapsedMilliseconds; // in milliseconds
                    if (elapsedMs < Pulse.PulseDelay)
                    {
                        //long elapsedTick = sw.ElapsedTicks; // 1 tick = 1 second/Stopwatch.Frequency
                        //long elapsedNs = sw.Elapsed.Ticks; // 1 tick = 1 nanosecond
                        //Log.Default.WriteLine(LogLevels.Debug, "Elapsed {0}Ms | {1}Ticks | {2}Ns", elapsedMs, elapsedTick, elapsedNs);
                        //Thread.Sleep(250 - (int) elapsedMs);
                        int sleepTime = (int)(Pulse.PulseDelay - elapsedMs);
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
