using Mud.Common;
using Mud.Domain;
using Mud.Logger;
using Mud.Network.Interfaces;
using Mud.Repository;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.World;
using Mud.Server.Random;
using Mud.Settings.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        protected ISettings Settings { get; }
        protected ILoginRepository LoginRepository { get; }
        protected IPlayerRepository PlayerRepository { get; }
        protected IAdminRepository AdminRepository { get; }
        protected IUniquenessManager UniquenessManager { get; }
        protected ITimeManager TimeManager { get; }
        protected IRandomManager RandomManager { get; }
        protected IGameActionManager GameActionManager { get; }
        protected IClassManager ClassManager { get; }
        protected IRaceManager RaceManager { get; }
        protected IAbilityManager AbilityManager { get; }
        protected IWorld World { get; }
        protected IRoomManager RoomManager { get; }
        protected ICharacterManager CharacterManager { get; }
        protected IItemManager ItemManager { get; }
        protected IQuestManager QuestManager { get; }

        public Server(ISettings settings,
            ILoginRepository loginRepository, IPlayerRepository playerRepository, IAdminRepository adminRepository,
            IUniquenessManager uniquenessManager, ITimeManager timeManager, IRandomManager randomManager, IGameActionManager gameActionManager,
            IClassManager classManager, IRaceManager raceManager, IAbilityManager abilityManager,
            IWorld world, IRoomManager roomManager, ICharacterManager characterManager, IItemManager itemManager, IQuestManager questManager)
        {
            Settings = settings;
            LoginRepository = loginRepository;
            PlayerRepository = playerRepository;
            AdminRepository = adminRepository;
            UniquenessManager = uniquenessManager;
            TimeManager = timeManager;
            RandomManager = randomManager;
            GameActionManager = gameActionManager;
            ClassManager = classManager;
            RaceManager = raceManager;
            AbilityManager = abilityManager;
            World = world;
            RoomManager = roomManager;
            CharacterManager = characterManager;
            ItemManager = itemManager;
            QuestManager = questManager;

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

            if (ItemManager.GetItemBlueprint<ItemCorpseBlueprint>(Settings.CorpseBlueprintId) == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Item corpse blueprint {0} not found or not a corpse", Settings.CorpseBlueprintId);
                throw new Exception($"Item corpse blueprint {Settings.CorpseBlueprintId} not found or not a corpse");
            }
            if (ItemManager.GetItemBlueprint<ItemMoneyBlueprint>(Settings.CoinsBlueprintId) == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Item coins blueprint {0} not found or not money", Settings.CoinsBlueprintId);
                throw new Exception($"Item coins blueprint {Settings.CoinsBlueprintId} not found or not money");
            }

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
            
            // Fix world
            World.FixWorld();

            // Reset world
            World.ResetWorld();
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
            LogLevels level = LogLevels.Info;
            if (flags.HasFlag(WiznetFlags.Bugs))
                level = LogLevels.Error;
            else if (flags.HasFlag(WiznetFlags.Typos))
                level = LogLevels.Warning;
            Log.Default.WriteLine(level, "WIZNET: FLAGS: {0} {1}", flags, message);
            foreach (IAdmin admin in Admins.Where(a => a.WiznetFlags.HasFlag(flags) && a.Level >= minLevel))
                admin.Send($"%W%WIZNET%x%:{message}");
        }

        #endregion

        #region IPlayerManager

        public IPlayer GetPlayer(ICommandParameter parameter, bool perfectMatch) => FindHelpers.FindByName(_players.Keys, parameter, perfectMatch);

        public IEnumerable<IPlayer> Players => _players.Keys;

        #endregion

        #region IAdminManager

        public IAdmin GetAdmin(ICommandParameter parameter, bool perfectMatch) => FindHelpers.FindByName(_players.Keys.OfType<IAdmin>(), parameter, perfectMatch);

        public IEnumerable<IAdmin> Admins => _players.Keys.OfType<IAdmin>();

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
            AdminData adminData = admin.MapPlayerData() as AdminData;
            AdminRepository.Save(adminData);

            // Save login
            LoginRepository.ChangeAdminStatus(admin.Name, true);

            // Inform admin about promotion
            admin.Send("You have been promoted to {0}", level);
        }

        #endregion

        #region IServerPlayerCommand

        public void Save(IPlayer player)
        {
            PlayingClient playingClient;
            _players.TryGetValue(player, out playingClient);
            if (playingClient == null)
                Log.Default.WriteLine(LogLevels.Error, "Save: client not found");
            else
            {
                if (playingClient.Player is IAdmin admin)
                {
                    AdminData data = admin.MapPlayerData() as AdminData;
                    AdminRepository.Save(data);
                    Log.Default.WriteLine(LogLevels.Info, $"Admin {playingClient.Player.DisplayName} saved");
                }
                else
                {
                    PlayerData data = playingClient.Player.MapPlayerData();
                    PlayerRepository.Save(data);
                    Log.Default.WriteLine(LogLevels.Info, $"Player {playingClient.Player.DisplayName} saved");
                }
            }
        }

        public void Quit(IPlayer player)
        {
            Save(player);
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

                // Keep player
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

            // Create a new player/admin only if not reconnecting
            if (playerOrAdmin == null)
            {
                //playerOrAdmin = isAdmin 
                //    ? new Admin.Admin(Guid.NewGuid(), username) 
                //    : new Player.Player(Guid.NewGuid(), username);
                if (isAdmin)
                {
                    AdminData data = AdminRepository.Load(username);
                    playerOrAdmin = new Admin.Admin(Guid.NewGuid(), data);
                }
                else
                {
                    PlayerData data = PlayerRepository.Load(username);
                    playerOrAdmin = new Player.Player(Guid.NewGuid(), data);
                }
                //
                playerOrAdmin.SendData += PlayerOnSendData;
                playerOrAdmin.PageData += PlayerOnPageData;
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
                Save(playerOrAdmin);

            // Prompt
            client.WriteData(playerOrAdmin.Prompt);

            // TODO: if new player, avatar creation state machine
            if (isNewPlayer)
            {
            }

            playerOrAdmin.ChangePlayerState(PlayerStates.Playing);
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
            foreach (PlayingClient playingClient in _players.Values.Shuffle(RandomManager)) // !! players list cannot be modified while processing inputs
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
                                    playingClient.Player.ProcessInput(command); // TODO: if command takes time to be processed, 'next' players will be delayed
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
            Log.Default.WriteLine(LogLevels.Info, "#Abilities: {0}", AbilityManager.Abilities.Count());
            Log.Default.WriteLine(LogLevels.Info, "#Passives: {0}", AbilityManager.Abilities.Count(x => x.Type == AbilityTypes.Passive));
            Log.Default.WriteLine(LogLevels.Info, "#Spells: {0}", AbilityManager.Abilities.Count(x => x.Type == AbilityTypes.Spell));
            Log.Default.WriteLine(LogLevels.Info, "#Skills: {0}", AbilityManager.Abilities.Count(x => x.Type == AbilityTypes.Skill));
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
                    foreach (IAbilityUsage abilityUsage in c.Abilities)
                        if (abilityUsage.ResourceKind.HasValue && !c.ResourceKinds.Contains(abilityUsage.ResourceKind.Value))
                            Log.Default.WriteLine(LogLevels.Warning, "Class {0} is allowed to use ability {1} [resource:{2}] but doesn't have access to that resource", c.DisplayName, abilityUsage.Name, abilityUsage.ResourceKind);
                }
            }
            Log.Default.WriteLine(LogLevels.Info, "#Classes: {0}", ClassManager.Classes.Count());
        }

        private void SanityCheckRaces()
        {
            Log.Default.WriteLine(LogLevels.Info, "#Races: {0}", RaceManager.PlayableRaces.Count());
        }

        private void SanityCheckQuests()
        {
            foreach (QuestBlueprint questBlueprint in QuestManager.QuestBlueprints)
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
            Log.Default.WriteLine(LogLevels.Info, "#QuestBlueprints: {0}", QuestManager.QuestBlueprints.Count);
        }

        private void SanityCheckRooms()
        {
            Log.Default.WriteLine(LogLevels.Info, "#RoomBlueprints: {0}", RoomManager.RoomBlueprints.Count);
            Log.Default.WriteLine(LogLevels.Info, "#Rooms: {0}", RoomManager.Rooms.Count());
        }

        private void SanityCheckItems()
        {
            Log.Default.WriteLine(LogLevels.Info, "#ItemBlueprints: {0}", ItemManager.ItemBlueprints.Count);
            Log.Default.WriteLine(LogLevels.Info, "#Items: {0}", ItemManager.Items.Count());
            if (ItemManager.GetItemBlueprint<ItemCorpseBlueprint>(Settings.CorpseBlueprintId) == null)
                Log.Default.WriteLine(LogLevels.Error, "Item corpse blueprint {0} not found or not a corpse", Settings.CorpseBlueprintId);
            if (ItemManager.GetItemBlueprint<ItemFoodBlueprint>(Settings.MushroomBlueprintId) == null)
                Log.Default.WriteLine(LogLevels.Error, "'a Magic mushroom' blueprint {0} not found or not food (needed for spell CreateFood)", Settings.MushroomBlueprintId);
            if (ItemManager.GetItemBlueprint<ItemFountainBlueprint>(Settings.SpringBlueprintId) == null)
                Log.Default.WriteLine(LogLevels.Error, "'a magical spring' blueprint {0} not found or not a fountain (needed for spell CreateSpring)", Settings.SpringBlueprintId);
            if (ItemManager.GetItemBlueprint<ItemLightBlueprint>(Settings.LightBallBlueprintId) == null)
                Log.Default.WriteLine(LogLevels.Error, "'a bright ball of light' blueprint {0} not found or not an light (needed for spell ContinualLight)", Settings.LightBallBlueprintId);
        }

        private void SanityCheckCharacters()
        {
            Log.Default.WriteLine(LogLevels.Info, "#CharacterBlueprints: {0}", CharacterManager.CharacterBlueprints.Count);
            Log.Default.WriteLine(LogLevels.Info, "#Characters: {0}", CharacterManager.Characters.Count());
        }

        private void DumpCommands()
        {
            //DumpCommandByType(typeof(Admin.Admin));
            //DumpCommandByType(typeof(Player.Player));
            //DumpCommandByType(typeof(NonPlayableCharacter));
            //DumpCommandByType(typeof(PlayableCharacter));
            //DumpCommandByType(typeof(Item.ItemBase<>));
            //DumpCommandByType(typeof(Room.Room));
            //Type actorBaseType = typeof(Actor.ActorBase);
            //var actorTypes = Assembly.GetExecutingAssembly().GetTypes()
            //    .Where(x => x.IsClass && !x.IsAbstract && actorBaseType.IsAssignableFrom(x))
            //    .ToList();
            //foreach (Type actorType in actorTypes)
            //    DumpCommandByType(actorType);
            StringBuilder sb = TableGenerators.GameActionInfoTableGenerator.Value.Generate($"Commands", GameActionManager.GameActions.OrderBy(x => x.Name));
            Log.Default.WriteLine(LogLevels.Debug, sb.ToString()); // Dump in log
        }

        //private void DumpCommandByType(Type t)
        //{
        //    for (char c = 'a'; c <= 'z'; c++)
        //    {
        //        IGameActionInfo[] query = GameActionManager.GetCommands(t).GetByPrefix(c.ToString()).Select(x => x.Value).OrderBy(x => x.Priority).ToArray();

        //        if (query.Length == 0)
        //            Log.Default.WriteLine(LogLevels.Debug, $"No commands for {t.Name} prefix '{c}'"); // Dump in log
        //        else
        //        {
        //            StringBuilder sb = TableGenerators.GameActionInfoTableGenerator.Value.Generate($"Commands for {t.Name} prefix '{c}'", query);
        //            Log.Default.WriteLine(LogLevels.Debug, sb.ToString()); // Dump in log
        //        }
        //    }
        //}

        private void DumpClasses()
        {
            StringBuilder sb = TableGenerators.ClassTableGenerator.Value.Generate("Classes", ClassManager.Classes.OrderBy(x => x.Name));
            Log.Default.WriteLine(LogLevels.Debug, sb.ToString()); // Dump in log
        }

        private void DumpRaces()
        {
            StringBuilder sb = TableGenerators.PlayableRaceTableGenerator.Value.Generate("Races", RaceManager.PlayableRaces.OrderBy(x => x.Name));
            Log.Default.WriteLine(LogLevels.Debug, sb.ToString()); // Dump in log
        }

        private void DumpAbilities()
        {
            StringBuilder sb = TableGenerators.FullInfoAbilityTableGenerator.Value.Generate("Abilities", AbilityManager.Abilities.OrderBy(x => x.Name));
            Log.Default.WriteLine(LogLevels.Debug, sb.ToString()); // Dump in log
        }

        private void HandleShutdown()
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

        private void HandleAggressiveNonPlayableCharacters()
        {
            IReadOnlyCollection<IPlayableCharacter> pcClone = new ReadOnlyCollection<IPlayableCharacter>(CharacterManager.PlayableCharacters.Where(x => x.Room != null).ToList()); // TODO: !immortal
            foreach (IPlayableCharacter pc in pcClone)
            {
                IReadOnlyCollection<INonPlayableCharacter> aggressorClone = new ReadOnlyCollection<INonPlayableCharacter>(pc.Room.NonPlayableCharacters.Where(x => !IsInvalidAggressor(x, pc)).ToList());
                foreach (INonPlayableCharacter aggressor in aggressorClone)
                {
                    var victims = aggressor.Room.PlayableCharacters.Where(x => IsValidVictim(x, aggressor)).ToArray();
                    if (victims.Length > 0)
                    {
                        IPlayableCharacter victim = RandomManager.Random(victims);
                        if (victim != null)
                        {
                            try
                            {
                                Log.Default.WriteLine(LogLevels.Debug, "HandleAggressiveNonPlayableCharacters: starting a fight between {0} and {1}", aggressor.DebugName, victim.DebugName);
                                aggressor.MultiHit(victim); // TODO: undefined type
                            }
                            catch (Exception ex)
                            {
                                Log.Default.WriteLine(LogLevels.Error, "Exception while handling aggressive behavior {0} on {1}. Exception: {2}", aggressor.DebugName, victim.DebugName, ex);
                            }
                        }
                    }
                }
            }
        }

        private bool IsInvalidAggressor(INonPlayableCharacter aggressor, IPlayableCharacter victim)
        {
            return 
                !aggressor.ActFlags.IsSet("Aggressive")
                || aggressor.Room.RoomFlags.IsSet("Safe")
                || aggressor.CharacterFlags.IsSet("Calm")
                || aggressor.Fighting != null
                || aggressor.CharacterFlags.IsSet("Charm")
                || aggressor.Position <= Positions.Sleeping
                || aggressor.ActFlags.IsSet("Wimpy") && victim.Position >= Positions.Sleeping // wimpy aggressive mobs only attack if player is asleep
                || !aggressor.CanSee(victim)
                || RandomManager.Chance(50);
        }

        private bool IsValidVictim(IPlayableCharacter victim, INonPlayableCharacter aggressor)
        {
            return
                // TODO: immortal
                aggressor.Level >= victim.Level - 5
                && (!aggressor.ActFlags.IsSet("Wimpy") || victim.Position < Positions.Sleeping) // wimpy aggressive mobs only attack if player is asleep
                && aggressor.CanSee(victim);
        }

        private void HandleAuras(int pulseCount) 
        {
            foreach (ICharacter character in CharacterManager.Characters.Where(x => x.Auras.Any(b => b.PulseLeft > 0)))
            {
                try
                {
                    bool needsRecompute = false;
                    IReadOnlyCollection<IAura> cloneAuras = new ReadOnlyCollection<IAura>(character.Auras.ToList()); // must be cloned because collection may be modified during foreach
                    foreach (IAura aura in cloneAuras.Where(x => x.PulseLeft >= 0))
                    {
                        bool timedOut = aura.DecreasePulseLeft(pulseCount);
                        if (timedOut)
                        {
                            //TODO: aura.OnVanished();
                            // TODO: Set Validity to false
                            character.RemoveAura(aura, false); // recompute once each aura has been processed
                            needsRecompute = true;
                        }
                        else if (aura.Level > 0 && RandomManager.Chance(20)) // spell strength fades with time
                            aura.DecreaseLevel();
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
            foreach (IItem item in ItemManager.Items.Where(x => x.Auras.Any(b => b.PulseLeft > 0)))
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
                        else if (aura.Level > 0 && RandomManager.Chance(20)) // spell strength fades with time
                            aura.DecreaseLevel();
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
            foreach (IRoom room in RoomManager.Rooms.Where(x => x.Auras.Any(b => b.PulseLeft > 0)))
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
            foreach (ICharacter character in CharacterManager.Characters.Where(x => x.HasAbilitiesInCooldown))
            {
                try
                {
                    IReadOnlyCollection<string> abilitiesInCooldown = new ReadOnlyCollection<string>(character.AbilitiesInCooldown.Keys.ToList()); // clone
                    foreach (string abilityName in abilitiesInCooldown)
                    {
                        bool available = character.DecreaseCooldown(abilityName, pulseCount);
                        if (available)
                            character.ResetCooldown(abilityName, true);
                    }
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
                    IReadOnlyCollection<IQuest> clone = new ReadOnlyCollection<IQuest>(player.Impersonating.Quests.Where(x => x.Blueprint.TimeLimit > 0).ToList()); // clone because quest list may be modified
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

        private void HandleViolence(int pulseCount)
        {
            //Log.Default.WriteLine(LogLevels.Debug, "HandleViolence: {0}", DateTime.Now);
            IReadOnlyCollection<ICharacter> clone = new ReadOnlyCollection<ICharacter>(CharacterManager.Characters.Where(x => x.Fighting != null && x.Room != null).ToList()); // clone because multi hit could kill character and then modify list
            foreach (ICharacter character in clone)
            {
                INonPlayableCharacter npcCharacter = character as INonPlayableCharacter;
                IPlayableCharacter pcCharacter = character as IPlayableCharacter;

                ICharacter victim = character.Fighting;
                if (victim != null)
                {
                    try
                    {
                        if (character.Position > Positions.Sleeping && victim.Room == character.Room) // fight continue only if in the same room and awake
                        {
                            Log.Default.WriteLine(LogLevels.Debug, "Continue fight between {0} and {1}", character.DebugName, victim.DebugName);
                            character.MultiHit(victim);
                        }
                        else
                        {
                            Log.Default.WriteLine(LogLevels.Debug, "Stop fighting between {0} and {1}, because not in same room", character.DebugName, victim.DebugName);
                            character.StopFighting(false);
                            if (npcCharacter != null)
                            {
                                Log.Default.WriteLine(LogLevels.Debug, "Non-playable character stop fighting, resetting it");
                                npcCharacter.Reset();
                            }
                        }
                        // check auto-assist
                        IReadOnlyCollection<ICharacter> cloneInRoom = new ReadOnlyCollection<ICharacter>(character.Room.People.Where(x => x.Fighting == null && x.Position > Positions.Sleeping).ToList());
                        foreach (ICharacter inRoom in cloneInRoom)
                        {
                            INonPlayableCharacter npcInRoom = inRoom as INonPlayableCharacter;
                            IPlayableCharacter pcInRoom = inRoom as IPlayableCharacter;
                            // quick check for ASSIST_PLAYER
                            if (pcCharacter != null && npcInRoom != null && npcInRoom.AssistFlags.IsSet("Players")
                                && npcInRoom.Level + 6 > victim.Level)
                            {
                                npcInRoom.Act(ActOptions.ToAll, "{0:N} scream{0:v} and attack{0:v}!", npcInRoom);
                                npcInRoom.MultiHit(victim);
                                continue;
                            }
                            // PCs next
                            if (pcCharacter != null 
                                || character.CharacterFlags.IsSet("Charm"))
                            {
                                bool isPlayerAutoassisting = pcInRoom != null && pcInRoom.AutoFlags.HasFlag(AutoFlags.Assist) && pcInRoom != null && pcInRoom.IsSameGroupOrPet(character);
                                bool isNpcAutoassisting = npcInRoom != null && npcInRoom.CharacterFlags.IsSet("Charm") && npcInRoom.Master == pcCharacter;
                                if ((isPlayerAutoassisting || isNpcAutoassisting)
                                    && victim.IsSafe(inRoom) == null)
                                {
                                    inRoom.MultiHit(victim);
                                    continue;
                                }
                            }
                            // now check the NPC cases
                            if (npcCharacter != null && !npcCharacter.CharacterFlags.IsSet("Charm")
                                && npcInRoom != null)
                            {
                                bool isAssistAll = npcInRoom.AssistFlags.IsSet("All");
                                bool isAssistGroup = false; // TODO
                                bool isAssistRace = npcInRoom.AssistFlags.IsSet("Race") && npcInRoom.Race == npcCharacter.Race;
                                bool isAssistAlign = npcInRoom.AssistFlags.IsSet("Align") && ((npcInRoom.IsGood && npcCharacter.IsGood) || (npcInRoom.IsNeutral && npcCharacter.IsNeutral) || (npcInRoom.IsEvil && npcCharacter.IsEvil));
                                bool isAssistVnum = npcInRoom.AssistFlags.IsSet("Vnum") && npcInRoom.Blueprint.Id == npcCharacter.Blueprint.Id;
                                if (isAssistAll || isAssistGroup || isAssistRace || isAssistAlign || isAssistVnum)
                                {
                                    if (RandomManager.Chance(50))
                                    {
                                        ICharacter target = character.Room.People.Where(x => npcInRoom.CanSee(x) && x.IsSameGroupOrPet(victim)).Random(RandomManager);
                                        if (target != null)
                                        {
                                            npcInRoom.Act(ActOptions.ToAll, "{0:N} scream{0:v} and attack{0:v}!", npcInRoom);
                                            npcInRoom.MultiHit(target);
                                            continue;
                                        }
                                    }
                                }
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

        private void HandleCharacters(int pulseCount)
        {
            foreach (ICharacter character in CharacterManager.Characters)
            {
                try
                {
                    IPlayableCharacter pc = character as IPlayableCharacter;
                    if (pc != null && pc.ImpersonatedBy == null) // TODO: remove after x minutes
                        Log.Default.WriteLine(LogLevels.Warning, "Impersonable {0} is not impersonated", character.DebugName);

                    // TODO: check to see if need to go home
                    // Update resources
                    character.Regen();

                    // Light
                    IItemLight light = character.GetEquipment<IItemLight>(EquipmentSlots.Light);
                    if (light != null
                        && light.IsLighten)
                    {
                        bool turnedOff = light.DecreaseTimeLeft();
                        if (turnedOff && character.Room != null)
                        {
                            character.Room.DecreaseLight();
                            character.Act(ActOptions.ToRoom, "{0} goes out.", light);
                            character.Act(ActOptions.ToCharacter, "{0} flickers and goes out.", light);
                            ItemManager.RemoveItem(light);
                        }
                        else if (!light.IsInfinite && light.TimeLeft < 5)
                            character.Act(ActOptions.ToCharacter, "{0} flickers.", light);
                    }

                    // Update conditions
                    pc?.GainCondition(Conditions.Drunk, -1); // decrease drunk state
                    // TODO: not if undead from here
                    pc?.GainCondition(Conditions.Full, character.Size > Sizes.Medium ? -4 : -2);
                    pc?.GainCondition(Conditions.Thirst, -1);
                    pc?.GainCondition(Conditions.Hunger, character.Size > Sizes.Medium ? -2 : -1);

                    // apply a random periodic affect if any
                    IAura[] periodicAuras = character.Auras.Where(x => x.Affects.Any(a => a is ICharacterPeriodicAffect)).ToArray();
                    if (periodicAuras.Length > 0)
                    {
                        IAura aura = periodicAuras.Random(RandomManager);
                        ICharacterPeriodicAffect affect = aura.Affects.OfType<ICharacterPeriodicAffect>().FirstOrDefault();
                        affect?.Apply(aura, character);
                    }

                    // TODO: limbo
                    // TODO: autosave, autoquit
                }
                catch (Exception ex)
                { 
                    Log.Default.WriteLine(LogLevels.Error, "Exception while handling character {0}. Exception: {1}", character.DebugName, ex);
                }
            }
        }

        private void HandleNonPlayableCharacters(int pulseCount)
        {
            foreach (INonPlayableCharacter npc in CharacterManager.NonPlayableCharacters.Where(x => x.IsValid && x.Room != null && !x.CharacterFlags.IsSet("Charm")))
            {
                try
                {
                    // is mob update always or area not empty
                    if (npc.ActFlags.IsSet("UpdateAlways") || npc.Room.Area.PlayableCharacters.Any())
                    {
                        // TODO: invoke spec_fun

                        // give some money to shopkeeper
                        if (npc.Blueprint is CharacterShopBlueprint)
                        {
                            if (npc.SilverCoins + npc.GoldCoins * 100 < npc.Blueprint.Wealth)
                            {
                                long silver = npc.Blueprint.Wealth * RandomManager.Range(1, 20) / 5000000;
                                long gold = npc.Blueprint.Wealth * RandomManager.Range(1, 20) / 50000;
                                if (silver > 0 || gold > 0)
                                {
                                    Log.Default.WriteLine(LogLevels.Debug, "Giving {0} silver {1} gold to {2}.", silver, gold, npc.DebugName);
                                    npc.UpdateMoney(silver, gold);
                                }
                            }
                        }

                        // that's all for all sleeping/busy monsters
                        if (npc.Position != Positions.Standing)
                            continue;

                        // scavenger
                        if (npc.ActFlags.IsSet("Scavenger") && npc.Room.Content.Any() && RandomManager.Range(0, 63) == 0)
                        {
                            //Log.Default.WriteLine(LogLevels.Debug, "Server.HandleNonPlayableCharacters: scavenger {0} on action", npc.DebugName);
                            // get most valuable item in room
                            IItem mostValuable = npc.Room.Content.Where(x => !x.NoTake && x.Cost > 0 /*&& CanLoot(npc, item)*/).OrderByDescending(x => x.Cost).FirstOrDefault();
                            if (mostValuable != null)
                            {
                                npc.Act(ActOptions.ToRoom, "{0} gets {1}.", npc, mostValuable);
                                mostValuable.ChangeContainer(npc);
                            }
                        }

                        // sentinel
                        if (!npc.ActFlags.IsSet("Sentinel") && RandomManager.Range(0, 7) == 0)
                        {
                            //Log.Default.WriteLine(LogLevels.Debug, "Server.HandleNonPlayableCharacters: sentinel {0} on action", npc.DebugName);
                            int exitNumber = RandomManager.Range(0, 31);
                            if (exitNumber < EnumHelpers.GetCount<ExitDirections>())
                            {
                                ExitDirections exitDirection = (ExitDirections)exitNumber;
                                IExit exit = npc.Room[exitDirection];
                                if (exit != null
                                    && exit.Destination != null
                                    && !exit.IsClosed
                                    && !exit.Destination.RoomFlags.IsSet("NoMob")
                                    && (!npc.ActFlags.IsSet("StayArea") || npc.Room.Area == exit.Destination.Area)
                                    && (!npc.ActFlags.IsSet("Outdoors") || !exit.Destination.RoomFlags.IsSet("Indoors"))
                                    && (!npc.ActFlags.IsSet("Indoors") || exit.Destination.RoomFlags.IsSet("Indoors")))
                                    npc.Move(exitDirection, false);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Exception while handling npc {0}. Exception: {1}", npc.DebugName, ex);
                }
            }
        }

        private void HandleItems(int pulseCount)
        {
            //Log.Default.WriteLine(LogLevels.Debug, "HandleItems {0} {1}", CurrentTime, DateTime.Now);
            IReadOnlyCollection<IItem> clone = new ReadOnlyCollection<IItem>(ItemManager.Items.Where(x => x.DecayPulseLeft > 0).ToList()); // clone bause decaying item will be removed from list
            foreach (IItem item in clone)
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
                        // TODO: give some money to shopkeeer
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
                                    IReadOnlyCollection<IItem> cloneContent = new ReadOnlyCollection<IItem>(container.Content.Where(x => !(x is IItemQuest)).ToList()); // except quest item
                                    foreach (IItem itemInContainer in cloneContent)
                                        itemInContainer.ChangeContainer(newContainer);
                                }
                            }
                        }
                        ItemManager.RemoveItem(item);
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
                foreach (IPlayableCharacter character in CharacterManager.PlayableCharacters.Where(x => 
                    x.Position > Positions.Sleeping 
                    && x.Room != null 
                    && !x.Room.RoomFlags.IsSet("Indoors")
                    && x.Room.SectorType != SectorTypes.Inside
                    && x.Room.SectorType != SectorTypes.Underwater))
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

        private void HandleAreas(int pulseCount)
        {
            World.ResetWorld();
        }

        private void Cleanup()
        {
            // Remove invalid entities
            World.Cleanup();
        }

        private void GameLoopTask()
        {
            PulseManager pulseManager = new PulseManager();
            pulseManager.Add(Pulse.PulsePerSeconds, Pulse.PulsePerSeconds, HandleAuras);
            pulseManager.Add(Pulse.PulsePerSeconds, Pulse.PulsePerSeconds, HandleCooldowns);
            pulseManager.Add(Pulse.PulsePerSeconds, Pulse.PulsePerSeconds, HandleQuests);
            pulseManager.Add(Pulse.PulsePerSeconds, Pulse.PulseViolence, HandleViolence);
            pulseManager.Add(Pulse.PulsePerSeconds, Pulse.PulsePerSeconds * 4, HandleNonPlayableCharacters);
            pulseManager.Add(Pulse.PulsePerSeconds, Pulse.PulsePerSeconds * 60, HandleCharacters);
            pulseManager.Add(Pulse.PulsePerSeconds, Pulse.PulsePerSeconds * 60, HandlePlayers);
            pulseManager.Add(Pulse.PulsePerSeconds, Pulse.PulsePerSeconds * 60, HandleItems);
            pulseManager.Add(Pulse.PulsePerSeconds, Pulse.PulsePerSeconds * 60, HandleRooms);
            pulseManager.Add(Pulse.PulsePerSeconds, Pulse.PulsePerSeconds * 60, HandleTime); // 1 minute IRL = 1 hour IG
            pulseManager.Add(Pulse.PulsePerMinutes * 3, Pulse.PulsePerMinutes * 3, HandleAreas);

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

                    HandleShutdown();
                    pulseManager.Pulse();
                    HandleAggressiveNonPlayableCharacters();

                    ProcessOutput();

                    Cleanup();

                    sw.Stop();
                    long elapsedMs = sw.ElapsedMilliseconds; // in milliseconds
                    if (elapsedMs < Pulse.PulseDelay)
                    {
                        int sleepTime = (int)(Pulse.PulseDelay - elapsedMs); // game loop should iterate every 250ms
                        //long elapsedTick = sw.ElapsedTicks; // 1 tick = 1 second/Stopwatch.Frequency
                        //long elapsedNs = sw.Elapsed.Ticks; // 1 tick = 1 nanosecond
                        //Log.Default.WriteLine(LogLevels.Debug, "Elapsed {0}ms | {1}ticks | {2}ns -> sleep {3}ms", elapsedMs, elapsedTick, elapsedNs, sleepTime);
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
