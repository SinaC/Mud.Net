using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Common;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Network.Interfaces;
using Mud.Repository.Interfaces;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.LootTable;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Blueprints.Reset;
using Mud.Server.Common;
using Mud.Server.Common.Helpers;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.World;
using Mud.Server.Options;
using Mud.Server.Random;
using Mud.Server.TableGenerator;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace Mud.Server.Server;

// Player lifecycle:
//  when INetworkServer detects a new connection, NewClientConnected is raised
//  a new login state machine is created/started and associated to client, client inputs/outputs are handled by login state machine instead if ProcessInput (via ClientLoginOnDataReceived)
//      --> client is considered as connecting
//  if login is failed, client is disconnected
//  if login is successful, login state machine is discarded, player/admin is created and client input/outputs are handled with ProcessInput/ProcessOutput
//      --> client is considered as playing

// Once playing,
//  in synchronous mode, input and output are 'queued' and handled by ProcessorInput/ProcessOutput
public class Server : IServer, IWorld, IPlayerManager, IServerAdminCommand, IServerPlayerCommand, IDisposable
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

    private readonly List<TreasureTable<int>> _treasureTables;

    private ILogger<Server> Logger { get; }
    private ILoginRepository LoginRepository { get; }
    private IPlayerRepository PlayerRepository { get; }
    private IAdminRepository AdminRepository { get; }
    private IUniquenessManager UniquenessManager { get; }
    private ITimeManager TimeManager { get; }
    private IRandomManager RandomManager { get; }
    private IGameActionManager GameActionManager { get; }
    private ICommandParser CommandParser { get; }
    private IClassManager ClassManager { get; }
    private IRaceManager RaceManager { get; }
    private IAbilityManager AbilityManager { get; }
    private IWeaponEffectManager WeaponEffectManager { get; }
    private IAreaManager AreaManager { get; }
    private IRoomManager RoomManager { get; }
    private ICharacterManager CharacterManager { get; }
    private IItemManager ItemManager { get; }
    private IQuestManager QuestManager { get; }
    private IResetManager ResetManager { get; }
    private IAdminManager AdminManager { get; }
    private IWiznet Wiznet { get; }
    private IPulseManager PulseManager { get; }
    private IReadOnlyCollection<ISanityCheck> SanityChecks { get; }
    private ServerOptions ServerOptions { get; }
    private WorldOptions WorldOptions { get; }

    public Server(ILogger<Server> logger, IOptions<ServerOptions> serverOptions, IOptions<WorldOptions> worldOptions,
        ILoginRepository loginRepository, IPlayerRepository playerRepository, IAdminRepository adminRepository,
        IUniquenessManager uniquenessManager, ITimeManager timeManager, IRandomManager randomManager, IGameActionManager gameActionManager, ICommandParser commandParser,
        IClassManager classManager, IRaceManager raceManager, IAbilityManager abilityManager, IWeaponEffectManager weaponEffectManager,
        IAreaManager areaManager, IRoomManager roomManager, ICharacterManager characterManager, IItemManager itemManager, IQuestManager questManager, IResetManager resetManager,
        IAdminManager adminManager, IWiznet wiznet, IPulseManager pulseManager, IEnumerable<ISanityCheck> sanityChecks)
    {
        Logger = logger;
        ServerOptions = serverOptions.Value;
        WorldOptions = worldOptions.Value;
        LoginRepository = loginRepository;
        PlayerRepository = playerRepository;
        AdminRepository = adminRepository;
        UniquenessManager = uniquenessManager;
        TimeManager = timeManager;
        RandomManager = randomManager;
        GameActionManager = gameActionManager;
        CommandParser = commandParser;
        ClassManager = classManager;
        RaceManager = raceManager;
        AbilityManager = abilityManager;
        WeaponEffectManager = weaponEffectManager;
        AreaManager = areaManager;
        RoomManager = roomManager;
        CharacterManager = characterManager;
        ItemManager = itemManager;
        QuestManager = questManager;
        ResetManager = resetManager;
        AdminManager = adminManager;
        Wiznet = wiznet;
        PulseManager = pulseManager;
        SanityChecks = sanityChecks.ToArray();

        _clients = new ConcurrentDictionary<IClient, PlayingClient>();
        _players = new ConcurrentDictionary<IPlayer, PlayingClient>();
        _loginInClients = new ConcurrentDictionary<IClient, LoginStateMachine>();
        _treasureTables = [];
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

        // TODO: check room specific id
        // TODO: move this in sanity checks
        if (ItemManager.GetItemBlueprint<ItemCorpseBlueprint>(WorldOptions.BlueprintIds.Corpse) == null)
        {
            Logger.LogError("Item corpse blueprint {corpseBlueprintId} not found or not a corpse", WorldOptions.BlueprintIds.Corpse);
            throw new Exception($"Item corpse blueprint {WorldOptions.BlueprintIds.Corpse} not found or not a corpse");
        }
        if (ItemManager.GetItemBlueprint<ItemMoneyBlueprint>(WorldOptions.BlueprintIds.Coins) == null)
        {
            Logger.LogError("Item coins blueprint {coinsBlueprintId} not found or not money", WorldOptions.BlueprintIds.Coins);
            throw new Exception($"Item coins blueprint {WorldOptions.BlueprintIds.Coins} not found or not money");
        }

        // Perform some validity/sanity checks
        if (ServerOptions.PerformSanityChecks)
        {
            PerformSanityChecks();
        }

        // Dump config
        if (ServerOptions.DumpOnInitialize)
            Dump();

        // TODO: other sanity checks
        // TODO: check room/item/character id uniqueness

        // Initialize UniquenessManager
        UniquenessManager.Initialize();
        
        // Fix world
        FixWorld();

        // Reset world
        ResetWorld();
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
            foreach (var networkServer in _networkServers)
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
            Logger.LogWarning("Operation canceled exception while stopping. Exception: {ex}", ex);
        }
        catch (AggregateException ex)
        {
            Logger.LogWarning("Aggregate exception while stopping. Exception: {ex}", ex.Flatten());
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

    #region IWorld

    public IReadOnlyCollection<TreasureTable<int>> TreasureTables => _treasureTables;

    public void AddTreasureTable(TreasureTable<int> table)
    {
        // TODO: check if already exists ?
        _treasureTables.Add(table);
    }

    public void FixWorld()
    {
        FixItems();
        FixResets();
    }

    public void ResetWorld()
    {
        foreach (IArea area in AreaManager.Areas)
        {
            // TODO: handle age + at load time, force age to arbitrary high value to ensure reset are computed
            //if (area.PlayableCharacters.Any())
            {
                ResetManager.ResetArea(area);
            }
        }
    }

    public void Cleanup() // remove invalid entities
    {
        RoomManager.Cleanup();
        CharacterManager.Cleanup();
        ItemManager.Cleanup();
    }

    private void FixItems()
    {
        Logger.LogInformation("Fixing items");
        foreach (var itemBlueprint in ItemManager.ItemBlueprints.OrderBy(x => x.Id))
        {
            switch (itemBlueprint)
            {
                case ItemLightBlueprint _:
                    if (itemBlueprint.WearLocation != WearLocations.Light)
                    {
                        Logger.LogError("Light {blueprintId} has wear location {location} -> set wear location to Light", itemBlueprint.Id, itemBlueprint.WearLocation);
                        itemBlueprint.WearLocation = WearLocations.Light;
                    }

                    break;
                case ItemWeaponBlueprint weaponBlueprint:
                    if (itemBlueprint.WearLocation != WearLocations.Wield && itemBlueprint.WearLocation != WearLocations.Wield2H)
                    {
                        var newWearLocation = WearLocations.Wield;
                        if (weaponBlueprint.Flags.IsSet("TwoHands"))
                            newWearLocation = WearLocations.Wield2H;
                        Logger.LogError("Weapon {blueprintId} has wear location {location} -> set wear location to {newLocation}", itemBlueprint.Id, itemBlueprint.WearLocation, newWearLocation);
                        itemBlueprint.WearLocation = newWearLocation;
                    }

                    break;
                case ItemShieldBlueprint _:
                    if (itemBlueprint.WearLocation != WearLocations.Shield)
                    {
                        Logger.LogError("Shield {blueprintId} has wear location {location} -> set wear location to Shield", itemBlueprint.Id, itemBlueprint.WearLocation);
                        itemBlueprint.WearLocation = WearLocations.Shield;
                    }

                    break;
                case ItemStaffBlueprint _:
                case ItemWandBlueprint _:
                    if (itemBlueprint.WearLocation != WearLocations.Hold)
                    {
                        Logger.LogError("{blueprintId} {blueprintType} has wear location {location} -> set wear location to Hold", itemBlueprint.ItemType(), itemBlueprint.Id, itemBlueprint.WearLocation);
                        itemBlueprint.WearLocation = WearLocations.Hold;
                    }

                    break;
            }
        }

        Logger.LogInformation("items fixed");
    }

    private void FixResets()
    {
        Logger.LogInformation("Fixing resets");

        // Global count is used to check global limit to 0
        Dictionary<int, int> characterResetGlobalCountById = [];
        Dictionary<int, int> itemResetCountGlobalById = [];

        foreach (var room in RoomManager.Rooms.Where(x => x.Blueprint.Resets?.Count > 0).OrderBy(x => x.Blueprint.Id))
        {
            Dictionary<int, int> characterResetCountById = [];
            Dictionary<int, int> itemResetCountById = [];

            // Count to check local limit  TODO: local limit is relative to container   example in dwarven.are Room 6534: item 6506 found with reset O and reset E -> 2 different containers
            foreach (ResetBase reset in room.Blueprint.Resets)
            {
                switch (reset)
                {
                    case CharacterReset characterReset:
                        characterResetCountById.Increment(characterReset.CharacterId);
                        characterResetGlobalCountById.Increment(characterReset.CharacterId);
                        break;
                    case ItemInRoomReset itemInRoomReset:
                        itemResetCountById.Increment(itemInRoomReset.ItemId);
                        itemResetCountGlobalById.Increment(itemInRoomReset.ItemId);
                        break;
                    case ItemInItemReset itemInItemReset:
                        itemResetCountById.Increment(itemInItemReset.ItemId);
                        itemResetCountGlobalById.Increment(itemInItemReset.ItemId);
                        break;
                    case ItemInCharacterReset itemInCharacterReset:
                        itemResetCountById.Increment(itemInCharacterReset.ItemId);
                        itemResetCountGlobalById.Increment(itemInCharacterReset.ItemId);
                        break;
                    case ItemInEquipmentReset itemInEquipmentReset:
                        itemResetCountById.Increment(itemInEquipmentReset.ItemId);
                        itemResetCountGlobalById.Increment(itemInEquipmentReset.ItemId);
                        break;
                }
            }

            // Check local limit + wear location
            foreach (var reset in room.Blueprint.Resets)
            {
                switch (reset)
                {
                    case CharacterReset characterReset:
                        {
                            int localCount = characterResetCountById[characterReset.CharacterId];
                            int localLimit = characterReset.LocalLimit;
                            if (localCount > localLimit)
                            {
                                Logger.LogError("Room {blueprintId}: M: character {characterId} found {count} times in room but local limit is {limit} -> modifying local limit to {newLimit}", room.Blueprint.Id, characterReset.CharacterId, localCount, localLimit, localCount);
                                characterReset.LocalLimit = localCount;
                            }

                            break;
                        }

                    case ItemInRoomReset itemInRoomReset:
                        {
                            int localCount = itemResetCountById[itemInRoomReset.ItemId];
                            int localLimit = itemInRoomReset.LocalLimit;
                            if (localCount > localLimit)
                            {
                                Logger.LogError("Room {blueprintId}: O: item {itemId} found {count} times in room but local limit is {limit} -> modifying local limit to {newLimit}", room.Blueprint.Id, itemInRoomReset.ItemId, localCount, localLimit, localCount);
                                itemInRoomReset.LocalLimit = localCount;
                            }

                            break;
                        }

                    case ItemInItemReset itemInItemReset:
                        {
                            int localCount = itemResetCountById[itemInItemReset.ItemId];
                            int localLimit = itemInItemReset.LocalLimit;
                            if (localCount > localLimit)
                            {
                                Logger.LogError("Room {blueprintId}: O: item {itemId} found {count} times in room but local limit is {limit} -> modifying local limit to {newLimit}", room.Blueprint.Id, itemInItemReset.ItemId, localCount, localLimit, localCount);
                                itemInItemReset.LocalLimit = localCount;
                            }

                            break;
                        }

                    case ItemInCharacterReset _: // no local limit check
                        break;
                    case ItemInEquipmentReset itemInEquipmentReset: // no local limit check but wear local check
                        {
                            // check wear location
                            var blueprint = ItemManager.GetItemBlueprint(itemInEquipmentReset.ItemId);
                            if (blueprint != null)
                            {
                                if (blueprint.WearLocation == WearLocations.None)
                                {
                                    var wearLocations = itemInEquipmentReset.EquipmentSlot.ToWearLocations().ToArray();
                                    var newWearLocation = wearLocations.FirstOrDefault(); // TODO: which one to choose from ?
                                    Logger.LogError("Room {blueprintId}: E: item {itemId} has no wear location but reset equipment slot {slot} -> modifying item wear location to {location}", room.Blueprint.Id, itemInEquipmentReset.ItemId, itemInEquipmentReset.EquipmentSlot, newWearLocation);
                                    blueprint.WearLocation = newWearLocation;
                                }
                                else
                                {
                                    var equipmentSlots = blueprint.WearLocation.ToEquipmentSlots().ToArray();
                                    if (equipmentSlots.All(x => x != itemInEquipmentReset.EquipmentSlot))
                                    {
                                        var newEquipmentSlot = equipmentSlots.First();
                                        Logger.LogError("Room {blueprintId}: E: item {itemId} reset equipment slot {slot} incompatible with wear location {location} -> modifying reset equipment slot to {newSlot}", room.Blueprint.Id, itemInEquipmentReset.ItemId, itemInEquipmentReset.EquipmentSlot, blueprint.WearLocation, newEquipmentSlot);
                                        itemInEquipmentReset.EquipmentSlot = newEquipmentSlot;
                                    }
                                }
                            }

                            break;
                        }
                }
            }
        }

        // Check global = 0 but found in reset
        foreach (var room in RoomManager.Rooms.Where(x => x.Blueprint.Resets?.Count > 0).OrderBy(x => x.Blueprint.Id))
        {
            foreach (var reset in room.Blueprint.Resets)
            {
                switch (reset)
                {
                    case CharacterReset characterReset:
                        {
                            int globalCount = characterResetGlobalCountById[characterReset.CharacterId];
                            if (characterReset.GlobalLimit == 0)
                            {
                                Logger.LogError("Room {blueprintId}: M: character {characterId} found {count} times in world but global limit is 0 -> modifying global limit to 1", room.Blueprint.Id, characterReset.CharacterId, globalCount);
                                characterReset.GlobalLimit = 1;
                            }
                            else if (characterReset.GlobalLimit != -1 && characterReset.GlobalLimit < globalCount)
                                Logger.LogWarning("Room {blueprintId}: M: character {characterId} found {count} times in world but global limit is {limit}", room.Blueprint.Id, characterReset.CharacterId, globalCount, characterReset.GlobalLimit);

                            break;
                        }

                    case ItemInRoomReset _: // no global count check
                        break;
                    case ItemInItemReset _: // no global count check
                        break;
                    case ItemInCharacterReset itemInCharacterReset:
                        {
                            int globalCount = itemResetCountGlobalById[itemInCharacterReset.ItemId];
                            if (itemInCharacterReset.GlobalLimit == 0)
                            {
                                Logger.LogError("Room {blueprintId}: G: item {itemId} found {count} times in world but global limit is 0 -> modifying global limit to 1", room.Blueprint.Id, itemInCharacterReset.ItemId, globalCount);
                                itemInCharacterReset.GlobalLimit = 1;
                            }
                            else if (itemInCharacterReset.GlobalLimit != -1 && itemInCharacterReset.GlobalLimit < globalCount)
                                Logger.LogWarning("Room {blueprintId}: G: item {itemId} found {count} times in world but global limit is {limit}", room.Blueprint.Id, itemInCharacterReset.ItemId, globalCount, itemInCharacterReset.GlobalLimit);

                            break;
                        }
                    case ItemInEquipmentReset itemInEquipmentReset:
                        {
                            int globalCount = itemResetCountGlobalById[itemInEquipmentReset.ItemId];
                            if (itemInEquipmentReset.GlobalLimit == 0)
                            {
                                Logger.LogError("Room {blueprintId}: E: item {itemId} found {count} times in world but global limit is 0 -> modifying global limit to 1", room.Blueprint.Id, itemInEquipmentReset.ItemId, globalCount);
                                itemInEquipmentReset.GlobalLimit = 1;
                            }
                            else if (itemInEquipmentReset.GlobalLimit != -1 && itemInEquipmentReset.GlobalLimit < globalCount)
                                Logger.LogWarning("Room {blueprintId}: E: item {itemId} found {count} times in world but global limit is {limit}", room.Blueprint.Id, itemInEquipmentReset.ItemId, globalCount, itemInEquipmentReset.GlobalLimit);

                            break;
                        }
                }
            }
        }

        Logger.LogInformation("Resets fixed");
    }

    #endregion

    #region IPlayerManager

    public IPlayer? GetPlayer(ICommandParameter parameter, bool perfectMatch)
        => FindHelpers.FindByName(_players.Keys, parameter, perfectMatch);

    public IEnumerable<IPlayer> Players
        => _players.Keys;

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
            Logger.LogError("Promote: client is already admin");
            return;
        }
        _players.TryGetValue(player, out var playingClient);
        if (playingClient == null)
        {
            Logger.LogError("Promote: client not found");
            return;
        }

        // Let's go
        Logger.LogInformation("Promoting {name} to {level}", player.Name, level);
        Wiznet.Log($"Promoting {player.Name} to {level}", WiznetFlags.Promote);

        // Remove from playing client
        lock (_playingClientLockObject)
        {
            _clients.TryRemove(playingClient.Client, out _);
            _players.TryRemove(playingClient.Player, out _);
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
        Admin.Admin admin = new (Logger, GameActionManager, CommandParser, TimeManager, CharacterManager, player.Id, player.Name, level, player.Aliases, player.Avatars);

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
            AdminManager.AddAdmin(admin);
        }

        // Delete player
        PlayerRepository.Delete(player.Name);

        // Save admin
        var adminData = admin.MapPlayerData() as AdminData;
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
        _players.TryGetValue(player, out var playingClient);
        if (playingClient == null)
            Logger.LogError("Save: client not found");
        else
        {
            if (playingClient.Player is IAdmin admin)
            {
                var data = admin.MapPlayerData() as AdminData;
                AdminRepository.Save(data);
                Logger.LogInformation($"Admin {playingClient.Player.DisplayName} saved");
            }
            else
            {
                var data = playingClient.Player.MapPlayerData();
                PlayerRepository.Save(data);
                Logger.LogInformation($"Player {playingClient.Player.DisplayName} saved");
            }
        }
    }

    public void Quit(IPlayer player)
    {
        Save(player);
        _players.TryGetValue(player, out var playingClient);
        if (playingClient == null)
            Logger.LogError("Quit: client not found");
        else
            ClientPlayingOnDisconnected(playingClient.Client);
    }

    public void Delete(IPlayer player)
    {
        _players.TryGetValue(player, out var playingClient);
        if (playingClient == null)
            Logger.LogError("Delete: client not found");
        else
        {
            string playerName = player.DisplayName;
            LoginRepository.DeleteLogin(player.Name);
            PlayerRepository.Delete(player.Name);
            UniquenessManager.RemoveAvatarNames(player.Avatars?.Select(x => x.Name));
            ClientPlayingOnDisconnected(playingClient.Client);
            //
            Logger.LogInformation("Player {name} has been deleted", playerName);
        }
    }

    #endregion

    #region Event handlers

    private void NetworkServerOnNewClientConnected(IClient client)
    {
        Logger.LogInformation("NetworkServerOnNewClientConnected");
        // Create/store a login state machine and starts it
        LoginStateMachine loginStateMachine = new (LoginRepository, UniquenessManager);
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
        Logger.LogInformation("NetworkServerOnClientDisconnected");
        _loginInClients.TryRemove(client, out var loginStateMachine);

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
        _loginInClients.TryGetValue(client, out var loginStateMachine);
        if (loginStateMachine != null)
            loginStateMachine.ProcessInput(client, command);
        else
            Logger.LogError("ClientLoginOnDataReceived: LoginStateMachine not found for a client!!!");
    }

    private void LoginStateMachineOnLoginSuccessful(IClient client, string username, bool isAdmin, bool isNewPlayer)
    {
        Logger.LogInformation("LoginStateMachineOnLoginSuccessful");

        IPlayer playerOrAdmin = null!;
        // if same user is already connected, remove old client and link new client to old player
        KeyValuePair<IPlayer, PlayingClient> previousPlayerPair;
        lock (_playingClientLockObject)
        {
            previousPlayerPair = _players.FirstOrDefault(x => x.Key.Name == username);
        }
        if (previousPlayerPair.Key != null)
        {
            Logger.LogInformation("Player was already connected, disconnect previous client and reuse player");

            // Keep player
            playerOrAdmin = previousPlayerPair.Key; // TODO: pause client ????
            // Remove client and player from players/clients
            lock (_playingClientLockObject)
            {
                bool removed = _players.TryRemove(playerOrAdmin, out var oldPlayingClient);
                if (removed && oldPlayingClient != null)
                    _clients.TryRemove(oldPlayingClient.Client, out _);
                // !!! PlayingClient removed from both collection must be equal
                if (playerOrAdmin is IAdmin admin)
                    AdminManager.RemoveAdmin(admin);
            }

            // Disconnect previous client
            previousPlayerPair.Value.Client.WriteData("Reconnecting on another client!!");
            previousPlayerPair.Value.Client.DataReceived -= ClientPlayingOnDataReceived;
            previousPlayerPair.Value.Client.Disconnect();

            Wiznet.Log($"{username} has reconnected.", WiznetFlags.Logins);

            // Welcome back
            client.WriteData("Reconnecting to Mud.Net!!" + Environment.NewLine);
        }
        else
        {
            Wiznet.Log($"{username} has connected.", WiznetFlags.Logins);

            // Welcome
            client.WriteData("Welcome to Mud.Net!!" + Environment.NewLine);
        }

        // Remove login state machine
        _loginInClients.TryRemove(client, out var loginStateMachine);
        if (loginStateMachine != null)
        {
            loginStateMachine.LoginFailed -= LoginStateMachineOnLoginFailed;
            loginStateMachine.LoginSuccessful -= LoginStateMachineOnLoginSuccessful;
        }
        else
            Logger.LogError("LoginStateMachineOnLoginSuccessful: LoginStateMachine not found for a client!!!");

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
                AdminData data = AdminRepository.Load(username) ?? new AdminData
                {
                    AdminLevel = AdminLevels.Angel,
                    Name = username,
                    PagingLineCount = 24,
                    WiznetFlags = WiznetFlags.None,
                    Aliases = [],
                    Characters = []
                };
                playerOrAdmin = new Admin.Admin(Logger, GameActionManager, CommandParser, TimeManager, CharacterManager, Guid.NewGuid(), data);
            }
            else
            {
                PlayerData data = PlayerRepository.Load(username) ?? new PlayerData
                {
                    Name = username,
                    PagingLineCount = 24,
                    Aliases = [],
                    Characters = []
                };
                playerOrAdmin = new Player.Player(Logger, GameActionManager, CommandParser, TimeManager, CharacterManager, Guid.NewGuid(), data);
            }
            //
            playerOrAdmin.SendData += PlayerOnSendData;
            playerOrAdmin.PageData += PlayerOnPageData;
        }
        //
        PlayingClient newPlayingClient = new(TimeManager)
        {
            Client = client,
            Player = playerOrAdmin
        };
        lock (_playingClientLockObject)
        {
            _players.TryAdd(playerOrAdmin, newPlayingClient);
            _clients.TryAdd(client, newPlayingClient);
            if (playerOrAdmin is IAdmin admin)
                AdminManager.AddAdmin(admin);
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
        PlayingClient? playingClient;
        lock (_playingClientLockObject)
            _clients.TryGetValue(client, out playingClient);
        if (playingClient == null)
            Logger.LogError("ClientPlayingOnDataReceived: null client");
        else if (command != null)
            playingClient.EnqueueReceivedData(command);
    }

    private void ClientPlayingOnDisconnected(IClient client)
    {
        Logger.LogInformation("ClientPlayingOnDisconnected");

        PlayingClient? playingClient;
        bool removed;
        lock (_playingClientLockObject)
        {
            removed = _clients.TryRemove(client, out playingClient);
            if (removed && playingClient != null)
            {
                _players.TryRemove(playingClient.Player, out playingClient);
                if (playingClient?.Player is IAdmin admin)
                    AdminManager.RemoveAdmin(admin);
            }
            // !!! PlayingClient removed from both collection must be equal
        }

        if (!removed)
            Logger.LogError("ClientPlayingOnDisconnected: playingClient not found!!!");
        else
        {
            Wiznet.Log($"{playingClient!.Player.DisplayName} has disconnected.", WiznetFlags.Logins);

            var admin = playingClient.Player as IAdmin;
            // Remove LastTeller and SnoopBy
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
        _players.TryGetValue(player, out var playingClient);
        if (playingClient == null)
            Logger.LogError("PlayerOnSendData: playingClient not found!!!");
        else
            playingClient.EnqueueDataToSend(data);
    }

    private void PlayerOnPageData(IPlayer player, StringBuilder data)
    {
        bool found = _players.TryGetValue(player, out var playingClient);
        if (!found || playingClient == null)
            Logger.LogError("PlayerOnPageData: playingClient not found!!!");
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
        foreach (var playingClient in _players.Values.Shuffle(RandomManager)) // !! players list cannot be modified while processing inputs
        {
            if (playingClient.Player != null)
            {
                string? command = null;
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

                    // handle save
                    if (playingClient.Player.SaveNeeded)
                    {
                        Save(playingClient.Player);
                        playingClient.Player.ResetSaveNeeded();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("Exception while processing input of {name} [{command}]. Exception: {ex}", playingClient.Player.Name, command ?? "???", ex);
                }
            }
            else
                Logger.LogError("ProcessInput: playing client without Player");
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
                    Logger.LogError("Exception while processing output of {name}. Exception: {ex}", playingClient.Player.Name, ex);
                }
            }
            else
                Logger.LogError("ProcessOutput: playing client without Player");
        }
    }

    private void PerformSanityChecks()
    {
        var fatalErrorFound = false;
        SanityCheckQuests();
        SanityCheckAbilities();
        SanityCheckClasses();
        SanityCheckRaces();
        SanityCheckRooms();
        SanityCheckItems();
        SanityCheckCharacters();
        SanityCheckWeaponEffects();
        foreach (var sanityCheck in SanityChecks)
        {
            fatalErrorFound |= sanityCheck.PerformSanityChecks();
        }
        if (fatalErrorFound)
            throw new Exception("Fatal sanity check fail detected. Stopping");
    }

    private void SanityCheckAbilities()
    {
        Logger.LogInformation("#Abilities: {count}", AbilityManager.Abilities.Count());
        Logger.LogInformation("#Passives: {count}", AbilityManager.Abilities.Count(x => x.Type == AbilityTypes.Passive));
        Logger.LogInformation("#Spells: {count}", AbilityManager.Abilities.Count(x => x.Type == AbilityTypes.Spell));
        Logger.LogInformation("#Skills: {count}", AbilityManager.Abilities.Count(x => x.Type == AbilityTypes.Skill));
    }

    private void SanityCheckClasses()
    {
        foreach (IClass c in ClassManager.Classes)
        {
            if (c.MaxHitPointGainPerLevel < c.MinHitPointGainPerLevel)
                Logger.LogWarning("Class {name} max hp per level < min hp per level", c.Name);
            if (c.ResourceKinds == null || !c.ResourceKinds.Any())
                Logger.LogWarning("Class {name} doesn't have any allowed resources", c.Name);
            else
            {
                foreach (IAbilityUsage abilityUsage in c.Abilities)
                    if (abilityUsage.ResourceKind.HasValue && !c.ResourceKinds.Contains(abilityUsage.ResourceKind.Value))
                        Logger.LogWarning("Class {name} is allowed to use ability {abilityName} [resource:{resource}] but doesn't have access to that resource", c.DisplayName, abilityUsage.Name, abilityUsage.ResourceKind);
            }
        }
        Logger.LogInformation("#Classes: {count}", ClassManager.Classes.Count());
    }

    private void SanityCheckRaces()
    {
        Logger.LogInformation("#Races: {count}", RaceManager.PlayableRaces.Count());
    }

    private void SanityCheckQuests()
    {
        foreach (QuestBlueprint questBlueprint in QuestManager.QuestBlueprints)
        {
            if (questBlueprint.ItemObjectives?.Length == 0 && questBlueprint.KillObjectives?.Length == 0 && questBlueprint.LocationObjectives?.Length == 0)
                Logger.LogError("Quest id {blueprintId} doesn't have any objectives.", questBlueprint.Id);
            else
            {
                var duplicateIds = (questBlueprint.ItemObjectives ?? Enumerable.Empty<QuestItemObjectiveBlueprint>()).Select(x => x.Id).Union((questBlueprint.KillObjectives ?? Enumerable.Empty<QuestKillObjectiveBlueprint>()).Select(x => x.Id)).Union((questBlueprint.LocationObjectives ?? Enumerable.Empty<QuestLocationObjectiveBlueprint>()).Select(x => x.Id))
                    .GroupBy(x => x, (id, ids) => new { objectiveId = id, count = ids.Count() }).Where(x => x.count > 1);
                foreach (var duplicateId in duplicateIds)
                    Logger.LogError("Quest id {blueprintId} has objectives with duplicate id {objectiveId} count {count}", questBlueprint.Id, duplicateId.objectiveId, duplicateId.count);
            }
        }
        Logger.LogInformation("#QuestBlueprints: {count}", QuestManager.QuestBlueprints.Count);
    }

    private void SanityCheckRooms()
    {
        Logger.LogInformation("#RoomBlueprints: {count}", RoomManager.RoomBlueprints.Count);
        Logger.LogInformation("#Rooms: {count}", RoomManager.Rooms.Count());
    }

    private void SanityCheckItems()
    {
        Logger.LogInformation("#ItemBlueprints: {count}", ItemManager.ItemBlueprints.Count);
        Logger.LogInformation("#Items: {count}", ItemManager.Items.Count());
        if (ItemManager.GetItemBlueprint<ItemCorpseBlueprint>(WorldOptions.BlueprintIds.Corpse) == null)
            Logger.LogError("Item corpse blueprint {blueprintId} not found or not a corpse", WorldOptions.BlueprintIds.Corpse);
        if (ItemManager.GetItemBlueprint<ItemMoneyBlueprint>(WorldOptions.BlueprintIds.Coins) == null)
            Logger.LogError("Item coins blueprint {blueprintId} not found or not money", WorldOptions.BlueprintIds.Coins);
        // TODO: stop server if no corpse or no money found
    }

    private void SanityCheckCharacters()
    {
        Logger.LogInformation("#CharacterBlueprints: {count}", CharacterManager.CharacterBlueprints.Count);
        Logger.LogInformation("#Characters: {count}", CharacterManager.Characters.Count());
    }

    private void SanityCheckWeaponEffects()
    {
        Logger.LogInformation("#WeaponEffects: {count}", WeaponEffectManager.Count);
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
        StringBuilder sb = TableGenerators.GameActionInfoTableGenerator.Value.Generate("Commands", GameActionManager.GameActions.OrderBy(x => x.Name));
        Logger.LogDebug(sb.ToString()); // Dump in log
    }

    //private void DumpCommandByType(Type t)
    //{
    //    for (char c = 'a'; c <= 'z'; c++)
    //    {
    //        IGameActionInfo[] query = GameActionManager.GetCommands(t).GetByPrefix(c.ToString()).Select(x => x.Value).OrderBy(x => x.Priority).ToArray();

    //        if (query.Length == 0)
    //            Logger.LogDebug($"No commands for {t.Name} prefix '{c}'"); // Dump in log
    //        else
    //        {
    //            StringBuilder sb = TableGenerators.GameActionInfoTableGenerator.Value.Generate($"Commands for {t.Name} prefix '{c}'", query);
    //            Logger.LogDebug(sb.ToString()); // Dump in log
    //        }
    //    }
    //}

    private void DumpClasses()
    {
        StringBuilder sb = TableGenerators.ClassTableGenerator.Value.Generate("Classes", ClassManager.Classes.OrderBy(x => x.Name));
        Logger.LogDebug(sb.ToString()); // Dump in log
    }

    private void DumpRaces()
    {
        StringBuilder sb = TableGenerators.PlayableRaceTableGenerator.Value.Generate("Races", RaceManager.PlayableRaces.OrderBy(x => x.Name));
        Logger.LogDebug(sb.ToString()); // Dump in log
    }

    private void DumpAbilities()
    {
        StringBuilder sb = TableGenerators.FullInfoAbilityTableGenerator.Value.Generate("Abilities", AbilityManager.Abilities.OrderBy(x => x.Name));
        Logger.LogDebug(sb.ToString()); // Dump in log
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
        var pcClone = new ReadOnlyCollection<IPlayableCharacter>(CharacterManager.PlayableCharacters.Where(x => x.Room != null).ToList()); // TODO: !immortal
        foreach (var pc in pcClone)
        {
            var aggressorClone = new ReadOnlyCollection<INonPlayableCharacter>(pc.Room.NonPlayableCharacters.Where(x => !IsInvalidAggressor(x, pc)).ToList());
            foreach (var aggressor in aggressorClone)
            {
                var victims = aggressor.Room.PlayableCharacters.Where(x => IsValidVictim(x, aggressor)).ToArray();
                if (victims.Length > 0)
                {
                    var victim = RandomManager.Random(victims);
                    if (victim != null)
                    {
                        try
                        {
                            Logger.LogDebug("HandleAggressiveNonPlayableCharacters: starting a fight between {aggressor} and {victim}", aggressor.DebugName, victim.DebugName);
                            aggressor.MultiHit(victim); // TODO: undefined type
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError("Exception while handling aggressive behavior {aggressor} on {victim}. Exception: {ex}", aggressor.DebugName, victim.DebugName, ex);
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
        foreach (var character in CharacterManager.Characters.Where(x => x.Auras.Any(b => b.PulseLeft > 0)))
        {
            try
            {
                bool needsRecompute = false;
                var cloneAuras = new ReadOnlyCollection<IAura>(character.Auras.ToList()); // must be cloned because collection may be modified during foreach
                foreach (var aura in cloneAuras.Where(x => x.PulseLeft >= 0))
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
                Logger.LogError("Exception while handling auras of character {name}. Exception: {ex}", character.DebugName, ex);
            }
        }
        foreach (var item in ItemManager.Items.Where(x => x.Auras.Any(b => b.PulseLeft > 0)))
        {
            try
            {
                bool needsRecompute = false;
                var cloneAuras = new ReadOnlyCollection<IAura>(item.Auras.ToList()); // must be cloned because collection may be modified during foreach
                foreach (var aura in cloneAuras.Where(x => x.PulseLeft > 0))
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
                Logger.LogError("Exception while handling auras of item {name}. Exception: {ex}", item.DebugName, ex);
            }
        }
        foreach (var room in RoomManager.Rooms.Where(x => x.Auras.Any(b => b.PulseLeft > 0)))
        {
            try
            {
                bool needsRecompute = false;
                var cloneAuras = new ReadOnlyCollection<IAura>(room.Auras.ToList()); // must be cloned because collection may be modified during foreach
                foreach (var aura in cloneAuras.Where(x => x.PulseLeft > 0))
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
                Logger.LogError("Exception while handling auras of room {name}. Exception: {ex}", room.DebugName, ex);
            }
        }
    }

    private void HandleCooldowns(int pulseCount) 
    {
        foreach (var character in CharacterManager.Characters.Where(x => x.HasAbilitiesInCooldown))
        {
            try
            {
                var abilitiesInCooldown = new ReadOnlyCollection<string>(character.AbilitiesInCooldown.Keys.ToList()); // clone
                foreach (string abilityName in abilitiesInCooldown)
                {
                    bool available = character.DecreaseCooldown(abilityName, pulseCount);
                    if (available)
                        character.ResetCooldown(abilityName, true);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception while handling cooldowns of {name}. Exception: {ex}", character.DebugName, ex);
            }
        }
    }

    private void HandleQuests(int pulseCount)
    {
        foreach (var player in Players.Where(x => x.Impersonating?.Quests?.Any(y => y.Blueprint.TimeLimit > 0) == true))
        {
            try
            {
                var clone = new ReadOnlyCollection<IQuest>(player.Impersonating!.Quests.Where(x => x.Blueprint.TimeLimit > 0).ToList()); // clone because quest list may be modified
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
                Logger.LogError("Exception while handling cooldowns of {name}. Exception: {ex}", player.Impersonating!.DebugName, ex);
            }
        }
    }

    private void HandleViolence(int pulseCount)
    {
        //Logger.LogDebug("HandleViolence: {0}", DateTime.Now);
        var clone = new ReadOnlyCollection<ICharacter>(CharacterManager.Characters.Where(x => x.Fighting != null && x.Room != null).ToList()); // clone because multi hit could kill character and then modify list
        foreach (var character in clone)
        {
            var npcCharacter = character as INonPlayableCharacter;
            var pcCharacter = character as IPlayableCharacter;

            var victim = character.Fighting;
            if (victim != null)
            {
                try
                {
                    if (character.Position > Positions.Sleeping && victim.Room == character.Room) // fight continue only if in the same room and awake
                    {
                        Logger.LogDebug("Continue fight between {character} and {victim}", character.DebugName, victim.DebugName);
                        character.MultiHit(victim);
                    }
                    else
                    {
                        Logger.LogDebug("Stop fighting between {character} and {victim}, because not in same room", character.DebugName, victim.DebugName);
                        character.StopFighting(false);
                        if (npcCharacter != null)
                        {
                            Logger.LogDebug("Non-playable character stop fighting, resetting it");
                            npcCharacter.Reset();
                        }
                    }
                    // check auto-assist
                    var cloneInRoom = new ReadOnlyCollection<ICharacter>(character.Room.People.Where(x => x.Fighting == null && x.Position > Positions.Sleeping).ToList());
                    foreach (var inRoom in cloneInRoom)
                    {
                        var npcInRoom = inRoom as INonPlayableCharacter;
                        var pcInRoom = inRoom as IPlayableCharacter;
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
                                    var target = character.Room.People.Where(x => npcInRoom.CanSee(x) && x.IsSameGroupOrPet(victim)).Random(RandomManager);
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
                    Logger.LogError("Exception while handling violence {character}. Exception: {ex}", character.DebugName, ex);
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

                // TODO: autosave once in a while, each loop save 10% of players
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception while handling player {name}. Exception: {ex}", playingClient.Player.Name, ex);
            }
        }
    }

    private void HandleCharacters(int pulseCount)
    {
        foreach (var character in CharacterManager.Characters)
        {
            try
            {
                var pc = character as IPlayableCharacter;
                if (pc != null && pc.ImpersonatedBy == null) // TODO: remove after x minutes
                    Logger.LogWarning("Impersonable {name} is not impersonated", character.DebugName);

                // TODO: check to see if need to go home
                // Update resources
                character.Regen();

                // Light
                var light = character.GetEquipment<IItemLight>(EquipmentSlots.Light);
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
                var periodicAuras = character.Auras.Where(x => x.Affects.Any(a => a is ICharacterPeriodicAffect)).ToArray();
                if (periodicAuras.Length > 0)
                {
                    var aura = periodicAuras.Random(RandomManager);
                    if (aura != null)
                    {
                        var affect = aura.Affects.OfType<ICharacterPeriodicAffect>().FirstOrDefault();
                        affect?.Apply(aura, character);
                    }
                }

                // TODO: limbo
                // TODO: autosave, autoquit
            }
            catch (Exception ex)
            { 
                Logger.LogError("Exception while handling character {name}. Exception: {ex}", character.DebugName, ex);
            }
        }
    }

    private void HandleNonPlayableCharacters(int pulseCount)
    {
        foreach (var npc in CharacterManager.NonPlayableCharacters.Where(x => x.IsValid && x.Room != null && !x.CharacterFlags.IsSet("Charm")))
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
                                Logger.LogDebug("Giving {silver} silver {gold} gold to {name}.", silver, gold, npc.DebugName);
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
                        //Logger.LogDebug("Server.HandleNonPlayableCharacters: scavenger {0} on action", npc.DebugName);
                        // get most valuable item in room
                        var mostValuable = npc.Room.Content.Where(x => !x.NoTake && x.Cost > 0 /*&& CanLoot(npc, item)*/).OrderByDescending(x => x.Cost).FirstOrDefault();
                        if (mostValuable != null)
                        {
                            npc.Act(ActOptions.ToRoom, "{0} gets {1}.", npc, mostValuable);
                            mostValuable.ChangeContainer(npc);
                        }
                    }

                    // sentinel
                    if (!npc.ActFlags.IsSet("Sentinel") && RandomManager.Range(0, 7) == 0)
                    {
                        //Logger.LogDebug("Server.HandleNonPlayableCharacters: sentinel {0} on action", npc.DebugName);
                        int exitNumber = RandomManager.Range(0, 31);
                        if (exitNumber < EnumHelpers.GetCount<ExitDirections>())
                        {
                            var exitDirection = (ExitDirections)exitNumber;
                            var exit = npc.Room[exitDirection];
                            if (exit != null
                                && exit.Destination != null
                                && !exit.IsClosed
                                && !exit.Destination.RoomFlags.IsSet("NoMob")
                                && (!npc.ActFlags.IsSet("StayArea") || npc.Room.Area == exit.Destination.Area)
                                && (!npc.ActFlags.IsSet("Outdoors") || !exit.Destination.RoomFlags.IsSet("Indoors"))
                                && (!npc.ActFlags.IsSet("Indoors") || exit.Destination.RoomFlags.IsSet("Indoors")))
                                npc.Move(exitDirection, false, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception while handling npc {name}. Exception: {ex}", npc.DebugName, ex);
            }
        }
    }

    private void HandleItems(int pulseCount)
    {
        //Logger.LogDebug("HandleItems {0} {1}", CurrentTime, DateTime.Now);
        var clone = new ReadOnlyCollection<IItem>(ItemManager.Items.Where(x => x.DecayPulseLeft > 0).ToList()); // clone bause decaying item will be removed from list
        foreach (var item in clone)
        {
            try
            {
                //Logger.LogDebug($"HandleItems {item.DebugName} with {item.DecayPulseLeft} pulse left");
                item.DecreaseDecayPulseLeft(pulseCount);
                if (item.DecayPulseLeft == 0)
                {
                    Logger.LogDebug("Item {name} decays", item.DebugName);
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
                            Logger.LogDebug("Move item content to room");
                            var newContainer = item.ContainedInto;
                            if (newContainer == null)
                                Logger.LogError("Item was in the void");
                            else
                            {
                                var cloneContent = new ReadOnlyCollection<IItem>(container.Content.Where(x => !(x is IItemQuest)).ToList()); // except quest item
                                foreach (var itemInContainer in cloneContent)
                                    itemInContainer.ChangeContainer(newContainer);
                            }
                        }
                    }
                    ItemManager.RemoveItem(item);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception while handling item {name}. Exception: {ex}", item.DebugName, ex);
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
        Logger.LogDebug("HandleTime: {timeUpdate}", !string.IsNullOrWhiteSpace(timeUpdate) ? timeUpdate : "no update");
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
                    Logger.LogError("Exception while handling time character {name}. Exception: {ex}", character.DebugName, ex);
                }
            }
        }
    }

    private void HandleAreas(int pulseCount)
    {
        ResetWorld();
    }

    private void GameLoopTask()
    {
        PulseManager.Add("Auras", Pulse.PulsePerSeconds, Pulse.PulsePerSeconds, HandleAuras);
        PulseManager.Add("CDs", Pulse.PulsePerSeconds, Pulse.PulsePerSeconds, HandleCooldowns);
        PulseManager.Add("Quests", Pulse.PulsePerSeconds, Pulse.PulsePerSeconds, HandleQuests);
        PulseManager.Add("Violence", Pulse.PulsePerSeconds, Pulse.PulseViolence, HandleViolence);
        PulseManager.Add("NPCs", Pulse.PulsePerSeconds, Pulse.PulsePerSeconds * 4, HandleNonPlayableCharacters);
        PulseManager.Add("NPCs+PCs", Pulse.PulsePerSeconds, Pulse.PulsePerSeconds * 60, HandleCharacters);
        PulseManager.Add("PCs", Pulse.PulsePerSeconds, Pulse.PulsePerSeconds * 60, HandlePlayers);
        PulseManager.Add("Items", Pulse.PulsePerSeconds, Pulse.PulsePerSeconds * 60, HandleItems);
        PulseManager.Add("Rooms", Pulse.PulsePerSeconds, Pulse.PulsePerSeconds * 60, HandleRooms);
        PulseManager.Add("Time", Pulse.PulsePerSeconds, Pulse.PulsePerSeconds * 60, HandleTime); // 1 minute IRL = 1 hour IG
        PulseManager.Add("Areas", Pulse.PulsePerMinutes * 3, Pulse.PulsePerMinutes * 3, HandleAreas);

        try
        {
            Stopwatch tickStopwatch = new();
            Stopwatch stepStopwatch = new();
            while (true)
            {
                tickStopwatch.Restart();
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    Logger.LogInformation("Stop GameLoopTask requested");
                    break;
                }

                TimeManager.FixCurrentTime();

                stepStopwatch.Restart();
                ProcessInput();
                stepStopwatch.Stop();
                var inputElapsed = stepStopwatch.ElapsedMilliseconds;

                HandleShutdown();

                stepStopwatch.Restart();
                PulseManager.Pulse();
                stepStopwatch.Stop();
                var pulseElapsed = stepStopwatch.ElapsedMilliseconds;

                stepStopwatch.Restart();
                HandleAggressiveNonPlayableCharacters();
                stepStopwatch.Stop();
                var aggressiveNpcElapsed = stepStopwatch.ElapsedMilliseconds;

                stepStopwatch.Restart();
                ProcessOutput();
                stepStopwatch.Stop();
                var outputElapsed = stepStopwatch.ElapsedMilliseconds;

                stepStopwatch.Restart();
                Cleanup();
                stepStopwatch.Stop();
                var cleanupElapsed = stepStopwatch.ElapsedMilliseconds;

                tickStopwatch.Stop();
                long elapsedMs = tickStopwatch.ElapsedMilliseconds; // in milliseconds
                if (elapsedMs < Pulse.PulseDelay)
                {
                    int sleepTime = (int)(Pulse.PulseDelay - elapsedMs); // game loop should iterate every 250ms
                    //long elapsedTick = sw.ElapsedTicks; // 1 tick = 1 second/Stopwatch.Frequency
                    //long elapsedNs = sw.Elapsed.Ticks; // 1 tick = 1 nanosecond
                    //Logger.LogDebug("Elapsed {0}ms | {1}ticks | {2}ns -> sleep {3}ms", elapsedMs, elapsedTick, elapsedNs, sleepTime);
                    _cancellationTokenSource.Token.WaitHandle.WaitOne(sleepTime);
                }
                else
                {
                    Logger.LogError("!!! No sleep for GameLoopTask. Elapsed {duration}", elapsedMs);
                    Logger.LogError("Input ms: {inputElapsed}", inputElapsed);
                    Logger.LogError("pulse ms: {pulseElapsed}", pulseElapsed);
                    Logger.LogError("aggressive npc ms: {aggressiveNpcElapsed}", aggressiveNpcElapsed);
                    Logger.LogError("output ms: {outputElapsed}", outputElapsed);
                    Logger.LogError("cleanup ms: {cleanupElapsed}", cleanupElapsed);

                    _cancellationTokenSource.Token.WaitHandle.WaitOne(1);
                }
            }
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogError("GameLoopTask exception. Exception: {ex}", ex);
        }

        PulseManager.Clear();

        Logger.LogInformation("GameLoopTask stopped");
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
                _cancellationTokenSource = null!;
            }
        }
    }

    #endregion
}
