using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Character;
using Mud.Blueprints.Item;
using Mud.Blueprints.LootTable;
using Mud.Blueprints.Reset;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Domain.SerializationData.Avatar;
using Mud.Network.Interfaces;
using Mud.Repository.Interfaces;
using Mud.Server.Commands.Character.PlayableCharacter.Quest;
using Mud.Server.Common;
using Mud.Server.Common.Extensions;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Affect.Character;
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
using Mud.Server.Quest.Objectives;
using Mud.Server.Random;
using Mud.Server.TableGenerator;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace Mud.Server.Server;

// Player lifecycle:
//  when INetworkServer detects a new connection, NewClientConnected is raised
//  a new login state machine is created/started and associated to client, client inputs/outputs are handled by login state machine instead of ProcessInput (via ClientLoginOnDataReceived)
//      --> client is considered as connecting
//  if login is failed, client is disconnected
//  if login is successful, login state machine is discarded, player/admin is created and client input/outputs are handled with ProcessInput/ProcessOutput
//      --> client is considered as playing

// Once playing,
//  in synchronous mode, input and output are 'queued' and handled by ProcessorInput/ProcessOutput
[Shared]
[Export(typeof(IServer))]
[Export(typeof(IWorld))]
[Export(typeof(IPlayerManager))]
[Export(typeof(IServerAdminCommand))]
[Export(typeof(IServerPlayerCommand))]
public class Server : IServer, IWorld, IPlayerManager, IServerAdminCommand, IServerPlayerCommand, IDisposable
{
    // This allows fast lookup with client or player BUT both structures must be modified at the same time
    private readonly object _playingClientLockObject = new();
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
    private IServiceProvider ServiceProvider { get; }
    private IAccountRepository AccountRepository { get; }
    private IAvatarRepository AvatarRepository { get; }
    private IUniquenessManager UniquenessManager { get; }
    private ITimeManager TimeManager { get; }
    private IRandomManager RandomManager { get; }
    private IGameActionManager GameActionManager { get; }
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

    public Server(ILogger<Server> logger, IServiceProvider serviceProvider, IOptions<ServerOptions> serverOptions,
        IAccountRepository accountRepository, IAvatarRepository avatarRepository,
        IUniquenessManager uniquenessManager, ITimeManager timeManager, IRandomManager randomManager, IGameActionManager gameActionManager,
        IClassManager classManager, IRaceManager raceManager, IAbilityManager abilityManager, IWeaponEffectManager weaponEffectManager,
        IAreaManager areaManager, IRoomManager roomManager, ICharacterManager characterManager, IItemManager itemManager, IQuestManager questManager, IResetManager resetManager,
        IAdminManager adminManager, IWiznet wiznet, IPulseManager pulseManager, IEnumerable<ISanityCheck> sanityChecks)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;
        ServerOptions = serverOptions.Value;
        AccountRepository = accountRepository;
        AvatarRepository = avatarRepository;
        UniquenessManager = uniquenessManager;
        TimeManager = timeManager;
        RandomManager = randomManager;
        GameActionManager = gameActionManager;
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
        _networkServers = [];
        _cancellationTokenSource = null!;
        _gameLoopTask = null!;
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
        if (ServerOptions.PerformSanityChecks)
        {
            PerformSanityChecks();
        }

        DisplayStats();

        // Dump config
        if (ServerOptions.DumpOnInitialize)
            Dump();

        // TODO: other sanity checks
        // TODO: check room/item/character id uniqueness

        // Initialize UniquenessManager
        UniquenessManager.Initialize();

        // Initialize pet shops
        InitializePetShops();

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

    private void DisplayStats()
    {
        Logger.LogInformation("#WeaponEffects: {count}", WeaponEffectManager.Count);
        Logger.LogInformation("#Abilities: {count}", AbilityManager.Abilities.Count());
        Logger.LogInformation("#Weapons: {count}", AbilityManager.Abilities.Count(x => x.Type == AbilityTypes.Weapon));
        Logger.LogInformation("#Passives: {count}", AbilityManager.Abilities.Count(x => x.Type == AbilityTypes.Passive));
        Logger.LogInformation("#Spells: {count}", AbilityManager.Abilities.Count(x => x.Type == AbilityTypes.Spell));
        Logger.LogInformation("#Skills: {count}", AbilityManager.Abilities.Count(x => x.Type == AbilityTypes.Skill));
        Logger.LogInformation("#Classes: {count}", ClassManager.Classes.Count());
        Logger.LogInformation("#Races: {count}", RaceManager.PlayableRaces.Count());
        Logger.LogInformation("#QuestBlueprints: {count}", QuestManager.QuestBlueprints.Count);
        Logger.LogInformation("#RoomBlueprints: {count}", RoomManager.RoomBlueprints.Count);
        Logger.LogInformation("#Rooms: {count}", RoomManager.Rooms.Count());
        Logger.LogInformation("#CharacterBlueprints: {count}", CharacterManager.CharacterBlueprints.Count);
        Logger.LogInformation("#Characters: {count}", CharacterManager.Characters.Count());
        Logger.LogInformation("#ItemBlueprints: {count}", ItemManager.ItemBlueprints.Count);
        Logger.LogInformation("#Items: {count}", ItemManager.Items.Count());
    }

    private void InitializePetShops()
    {
        foreach (var petShopBlueprint in CharacterManager.CharacterBlueprints.OfType<CharacterPetShopBlueprint>())
        {
            foreach (var petBlueprintId in petShopBlueprint.PetBlueprintIds.Distinct())
            {
                var petBlueprint = CharacterManager.GetCharacterBlueprint<CharacterNormalBlueprint>(petBlueprintId);
                if (petBlueprint != null)
                    petShopBlueprint.PetBlueprints.Add(petBlueprint);
            }
        }
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
        var sb = TableGenerators.ClassTableGenerator.Value.Generate("Classes", ClassManager.Classes.OrderBy(x => x.Name));
        Logger.LogDebug(sb.ToString()); // Dump in log
    }

    private void DumpRaces()
    {
        var sb = TableGenerators.PlayableRaceTableGenerator.Value.Generate("Races", RaceManager.PlayableRaces.OrderBy(x => x.Name));
        Logger.LogDebug(sb.ToString()); // Dump in log
    }

    private void DumpAbilities()
    {
        var sb = TableGenerators.FullInfoAbilityTableGenerator.Value.Generate("Abilities", AbilityManager.Abilities.OrderBy(x => x.Name));
        Logger.LogDebug(sb.ToString()); // Dump in log
    }
    private void PerformSanityChecks()
    {
        var fatalErrorFound = false;
        foreach (var sanityCheck in SanityChecks)
        {
            fatalErrorFound |= sanityCheck.PerformSanityChecks();
        }
        if (fatalErrorFound)
            throw new Exception("Fatal sanity check fail detected. Stopping");
    }

    private void DumpCommands()
    {
        var sb = TableGenerators.GameActionInfoTableGenerator.Value.Generate("Commands", GameActionManager.GameActions.OrderBy(x => x.Name));
        Logger.LogDebug(sb.ToString()); // Dump in log
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
        foreach (var area in AreaManager.Areas)
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
        var admin = ServiceProvider.GetRequiredService<IAdmin>();
        admin.Initialize(player.Id, player.Name, player.Password, level, player.Aliases, player.AvatarMetaDatas);

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

        // Save admin
        var adminData = admin.MapAccountData();
        if (adminData != null)
            AccountRepository.Save(adminData);

        // Inform admin about promotion
        admin.Send("You have been promoted to {0}", level);
    }

    #endregion

    #region IServerPlayerCommand

    public AvatarData? LoadAvatar(string avatarName)
    {
        return AvatarRepository.Load(avatarName);
    }

    public void SaveAvatar(AvatarData avatar)
    {
        AvatarRepository.Save(avatar);
    }

    public void DeleteAvatar(string avatarName)
    {
        AvatarRepository.Delete(avatarName);
        UniquenessManager.RemoveAvatarName(avatarName);
    }

    public void Save(IPlayer player)
    {
        _players.TryGetValue(player, out var playingClient);
        if (playingClient == null)
            Logger.LogError("Save: client not found");
        else
        {
            Logger.LogInformation("Saving player {name}", playingClient.Player.DisplayName);
            if (playingClient.Player.Impersonating != null)
            {
                playingClient.Player.UpdateAvatarMetaDataFromImpersonated();
                var avatarData = playingClient.Player.Impersonating.MapAvatarData();
                AvatarRepository.Save(avatarData);
            }
            var accountData = playingClient.Player.MapAccountData();
            AccountRepository.Save(accountData);
            Logger.LogInformation("Player {name} saved", playingClient.Player.DisplayName);
        }
    }

    public void Quit(IPlayer player)
    {
        Save(player);
        _players.TryGetValue(player, out var playingClient);
        if (playingClient == null)
            Logger.LogError("Quit: client not found");
        else
        {
            ProcessOutput(playingClient, false);
            ClientPlayingOnDisconnected(playingClient.Client);
        }
    }

    public void Delete(IPlayer player)
    {
        _players.TryGetValue(player, out var playingClient);
        if (playingClient == null)
            Logger.LogError("Delete: client not found");
        else
        {
            // delete avatars
            foreach (var avatarMetaData in player.AvatarMetaDatas)
                AvatarRepository.Delete(avatarMetaData.Name);
            // delete player
            AccountRepository.Delete(player.Name);
            // remove avatar names and player name from uniqueness manager
            UniquenessManager.RemoveAvatarNames(player.AvatarMetaDatas?.Select(x => x.Name));
            UniquenessManager.RemoveAccountName(player.Name);
            //
            ProcessOutput(playingClient, false);
            ClientPlayingOnDisconnected(playingClient.Client);
            //
            Logger.LogInformation("Player {name} has been deleted", player.Name);
        }
    }

    #endregion

    #region Event handlers

    private void NetworkServerOnNewClientConnected(IClient client)
    {
        Logger.LogInformation("NetworkServerOnNewClientConnected");
        // Create/store a login state machine and starts it
        var loginStateMachine = ServiceProvider.GetRequiredService<LoginStateMachine>();
        if (loginStateMachine == null)
        {
            Logger.LogError("NetworkServerOnNewClientConnected: cannot create LoginStateMachine");
            client.WriteData(StringHelpers.SomethingGoesWrong);
            client.Disconnect();
            return; // stop with this client
        }
        _loginInClients.TryAdd(client, loginStateMachine);
        // Add login handlers
        loginStateMachine.LoginFailed += LoginStateMachineOnLoginFailed;
        loginStateMachine.LoginSuccessful += LoginStateMachineOnLoginSuccessful;
        client.DataReceived += ClientLoginOnDataReceived;
        // Send greetings
        loginStateMachine.Initialize(client);
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
            // load account data
            var accountData = AccountRepository.Load(username);
            if (accountData == null)
            {
                Logger.LogError("LoginStateMachineOnLoginSuccessful: Account {username} not found!!!", username);
                NetworkServerOnClientDisconnected(client); // forced to disconnect
                return;
            }
            // create player/admin and initialize
            if (isAdmin && accountData.AdminData != null)
            {
                playerOrAdmin = ServiceProvider.GetRequiredService<IAdmin>();
            }
            else
            {
                playerOrAdmin = ServiceProvider.GetRequiredService<IPlayer>();
            }
            playerOrAdmin.Initialize(Guid.NewGuid(), accountData);

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
        {
            UniquenessManager.AddAccountName(username);
            Save(playerOrAdmin);
        }

        // Prompt
        client.WriteData(playerOrAdmin.Prompt);

        // TODO: if new player, avatar creation state machine
        if (isNewPlayer)
        {
            // TODO
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
        {
            client.Disconnect();
            client.DataReceived -= ClientPlayingOnDataReceived;
            Logger.LogError("ClientPlayingOnDisconnected: playingClient not found!!!");
        }
        else
        {
            Wiznet.Log($"{playingClient!.Player.DisplayName} has disconnected.", WiznetFlags.Logins);

            var admin = playingClient.Player as IAdmin;
            // Remove LastTeller and SnoopBy
            foreach (var player in Players)
            {
                if (player.LastTeller == playingClient.Player)
                    player.SetLastTeller(null);
                if (admin != null && player.SnoopBy == admin)
                    player.SetSnoopBy(null);
            }
            playingClient.Player.OnDisconnected();
            client.Disconnect();
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
                    if (playingClient.Player.Lag > 0) // if player is on artificial lag, decrease it and skip input processing
                        playingClient.Player.DecreaseLag();
                    else if (playingClient.Player.Impersonating == null || playingClient.Player.Impersonating.GlobalCooldown <= 0) // if player is on GCD, don't process input
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
            ProcessOutput(playingClient, true);
        }
    }

    private void ProcessOutput(PlayingClient playingClient, bool displayPrompt)
    {
        if (playingClient.Player != null)
        {
            try
            {
                var data = playingClient.DequeueDataToSend(); // TODO should return a StringBuilder to quickly append prompt
                if (!string.IsNullOrWhiteSpace(data))
                {
                    // Add prompt
                    if (displayPrompt)
                    {
                        data += playingClient.Player.Prompt;
                    }
                    // Send datas
                    playingClient.Client.WriteData(data);
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
        // for each pc
        //   for each aggresive npc in pc room
        //     pick a random valid victim in room
        var pcs = CharacterManager.PlayableCharacters.Where(x => x.Room != null).ToArray(); // TODO: !immortal
        foreach (var pc in pcs)
        {
            var aggressors = pc.Room.NonPlayableCharacters.Where(x => !IsInvalidAggressor(x, pc)).ToArray();
            foreach (var aggressor in aggressors)
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

    private void HandleDazeAndWait()
    {
        foreach (var ch in CharacterManager.Characters)
        {
            // decrease daze
            var wasDazed = ch.Daze > 0;
            if (wasDazed)
            {
                Logger.LogTrace("DECREASE DAZE for {name} {daze} left", ch.DebugName, ch.Daze);
                ch.DecreaseDaze();
            }

            // decrease GCD
            var wasWaiting = ch.GlobalCooldown > 0;
            if (wasWaiting)
            {
                Logger.LogTrace("DECREASE GCD for {name} {gcd} left", ch.DebugName, ch.GlobalCooldown);
                ch.DecreaseGlobalCooldown();
            }

            // Attempt to stand back up and fight!
            if (wasDazed && ch.Daze == 0 && ch.GlobalCooldown == 0)
                ch.StandUpInCombatIfPossible();
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

    private static bool IsValidVictim(IPlayableCharacter victim, INonPlayableCharacter aggressor)
    {
        return
            // TODO: immortal
            aggressor.Level >= victim.Level - 5
            && (!aggressor.ActFlags.IsSet("Wimpy") || victim.Position < Positions.Sleeping) // wimpy aggressive mobs only attack if player is asleep
            && aggressor.CanSee(victim);
    }

    private void HandleAuras(int pulseCount) 
    {
        HandleAuras(CharacterManager.Characters, pulseCount);
        HandleAuras(ItemManager.Items, pulseCount);
        HandleAuras(RoomManager.Rooms, pulseCount);
    }

    private void HandleAuras<TEntity>(IEnumerable<TEntity> entities, int pulseCount)
        where TEntity : IEntity
    {
        var auraFilterFunc = new Func<IAura, bool>(a => !a.AuraFlags.HasFlag(AuraFlags.Permanent) && a.PulseLeft > 0);
        foreach (var entity in entities.Where(x => x.Auras.Any(a => !a.AuraFlags.HasFlag(AuraFlags.Permanent) && a.PulseLeft > 0)))
        {
            try
            {
                bool needsRecompute = false;
                var auras = entity.Auras.Where(auraFilterFunc).ToArray(); // must be cloned because collection may be modified during foreach
                foreach (var aura in auras)
                {
                    bool timedOut = aura.DecreasePulseLeft(pulseCount);
                    if (timedOut)
                    {
                        //TODO: aura.OnVanished();
                        // TODO: Set Validity to false
                        entity.RemoveAura(aura, false); // recompute once each aura has been processed
                        needsRecompute = true;
                    }
                    else if (aura.Level > 0 && RandomManager.Chance(20)) // spell strength fades with time
                        aura.DecreaseLevel();
                }
                if (needsRecompute)
                    entity.Recompute();
                // TODO: remove invalid auras
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception while handling auras of {entityType} {name}. Exception: {ex}", typeof(TEntity), entity.DebugName, ex);
            }
        }
    }

    private void HandleCooldowns(int pulseCount) 
    {
        foreach (var ch in CharacterManager.Characters.Where(x => x.HasAbilitiesInCooldown))
        {
            try
            {
                var abilitiesInCooldown = ch.AbilitiesInCooldown.Keys.ToArray(); // clone
                foreach (string abilityName in abilitiesInCooldown)
                {
                    var available = ch.DecreaseCooldown(abilityName, pulseCount);
                    if (available)
                        ch.ResetCooldown(abilityName, true);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception while handling cooldowns of {name}. Exception: {ex}", ch.DebugName, ex);
            }
        }
    }

    private void HandleQuests(int pulseCount)
    {
        foreach (var player in Players.Where(x => x.Impersonating != null))
        {
            var impersonating = player.Impersonating!;
            try
            {
                // quest time out ?
                if (impersonating.Quests.Any(x => x.TimeLimit > 0))
                {
                    var quests = impersonating.Quests.Where(x => x.TimeLimit > 0).ToArray(); // clone because quest list may be modified
                    foreach (var quest in quests)
                    {
                        var timedOut = quest.DecreasePulseLeft(pulseCount);
                        if (timedOut)
                        {
                            quest.Timeout();
                            impersonating.RemoveQuest(quest);
                        }
                    }
                }
                // decrease time until next automatic quest
                if (impersonating.PulseLeftBeforeNextAutomaticQuest > 0)
                {
                    impersonating.DecreasePulseLeftBeforeNextAutomaticQuest(pulseCount);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception while handling quests of {name}. Exception: {ex}", player.Impersonating!.DebugName, ex);
            }
        }
    }

    private void HandleViolence(int pulseCount)
    {
        //Logger.LogDebug("HandleViolence: {0}", DateTime.Now);
        var fightingCharacters = CharacterManager.Characters.Where(x => x.Fighting != null && x.Room != null).ToArray(); // clone because multi hit could kill character and then modify list
        foreach (var ch in fightingCharacters)
        {
            var npc = ch as INonPlayableCharacter;
            var pc = ch as IPlayableCharacter;

            var victim = ch.Fighting;
            if (victim != null)
            {
                try
                {
                    if (ch.Position > Positions.Sleeping && victim.Room == ch.Room) // fight continue only if in the same room and awake
                    {
                        Logger.LogDebug("Continue fight between {character} and {victim}", ch.DebugName, victim.DebugName);
                        ch.MultiHit(victim);
                    }
                    else
                    {
                        Logger.LogDebug("Stop fighting between {character} and {victim}, because not in same room", ch.DebugName, victim.DebugName);
                        ch.StopFighting(false);
                        if (npc != null)
                        {
                            Logger.LogDebug("Non-playable character stop fighting, resetting it");
                            npc.Reset(); // TODO
                        }
                    }
                    // check auto-assist
                    var cloneInRoom = ch.Room.People.Where(x => x.Fighting == null && x.Position > Positions.Sleeping).ToArray();
                    foreach (var inRoom in cloneInRoom)
                    {
                        var npcInRoom = inRoom as INonPlayableCharacter;
                        var pcInRoom = inRoom as IPlayableCharacter;
                        // quick check for ASSIST_PLAYER
                        if (pc != null && npcInRoom != null && npcInRoom.AssistFlags.IsSet("Players")
                            && npcInRoom.Level + 6 > victim.Level)
                        {
                            npcInRoom.Act(ActOptions.ToAll, "{0:N} scream{0:v} and attack{0:v}!", npcInRoom);
                            npcInRoom.MultiHit(victim);
                            continue;
                        }
                        // PCs next
                        if (pc != null 
                            || ch.CharacterFlags.IsSet("Charm"))
                        {
                            var isPlayerAutoassisting = pcInRoom != null && pcInRoom.AutoFlags.HasFlag(AutoFlags.Assist) && pcInRoom != null && pcInRoom.IsSameGroupOrPet(ch);
                            var isNpcAutoassisting = npcInRoom != null && npcInRoom.CharacterFlags.IsSet("Charm") && npcInRoom.Master == pc;
                            if ((isPlayerAutoassisting || isNpcAutoassisting)
                                && victim.IsSafe(inRoom) == null)
                            {
                                inRoom.MultiHit(victim);
                                continue;
                            }
                        }
                        // now check the NPC cases
                        if (npc != null && !npc.CharacterFlags.IsSet("Charm")
                            && npcInRoom != null)
                        {
                            var isAssistAll = npcInRoom.AssistFlags.IsSet("All");
                            var isAssistGroup = npcInRoom.Blueprint.Group != 0 && npcInRoom.Blueprint.Group == npc.Blueprint.Group; // group assist
                            var isAssistRace = npcInRoom.AssistFlags.IsSet("Race") && npcInRoom.Race == npc.Race;
                            var isAssistAlign = npcInRoom.AssistFlags.IsSet("Align") && ((npcInRoom.IsGood && npc.IsGood) || (npcInRoom.IsNeutral && npc.IsNeutral) || (npcInRoom.IsEvil && npc.IsEvil));
                            // TODO: assist guard
                            var isAssistVnum = npcInRoom.AssistFlags.IsSet("Vnum") && npcInRoom.Blueprint.Id == npc.Blueprint.Id;
                            if (isAssistAll || isAssistGroup || isAssistRace || isAssistAlign || isAssistVnum)
                            {
                                if (RandomManager.Chance(50))
                                {
                                    var target = ch.Room.People.Where(x => npcInRoom.CanSee(x) && x.IsSameGroupOrPet(victim)).Random(RandomManager);
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
                    Logger.LogError("Exception while handling violence {character}. Exception: {ex}", ch.DebugName, ex);
                }
            }
        }
    }

    private void HandleResources(int pulseCount)
    {
        foreach (var ch in CharacterManager.Characters)
        {
            ch.Regen(pulseCount);
        }
    }

    private void HandlePlayers(int pulseCount)
    {
        foreach (var playingClient in _players.Values)
        {
            try
            {
                // If idle for too long, unimpersonate or disconnect
                var ts = TimeManager.CurrentTime - playingClient.LastReceivedDataTimestamp;
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

                // TODO: autosave once in a while, each loop would save 10% of players
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception while handling player {name}. Exception: {ex}", playingClient.Player.Name, ex);
            }
        }
    }

    private void HandleCharacters(int pulseCount)
    {
        foreach (var ch in CharacterManager.Characters)
        {
            try
            {
                var pc = ch as IPlayableCharacter;
                if (pc != null && pc.ImpersonatedBy == null) // TODO: remove after x minutes
                    Logger.LogWarning("Impersonable {name} is not impersonated", ch.DebugName);

                // TODO: check to see if need to go home
                /*
                 *  if (IS_NPC(ch) && ch->zone != NULL && ch->zone != ch->in_room->area
            && ch->desc == NULL &&  ch->fighting == NULL 
	    && !IS_AFFECTED(ch,AFF_CHARM) && number_percent() < 5)
            {
            	act("$n wanders on home.",ch,NULL,NULL,TO_ROOM);
            	extract_char(ch,TRUE);
            	continue;
            }*/

                // regen is now handled in HandleResources
                //// Update resources
                //character.Regen();

                // Light
                var light = ch.GetEquipment<IItemLight>(EquipmentSlots.Light);
                if (light != null
                    && light.IsLighten)
                {
                    bool turnedOff = light.DecreaseTimeLeft();
                    if (turnedOff && ch.Room != null)
                    {
                        ch.Room.DecreaseLight();
                        ch.Act(ActOptions.ToRoom, "{0} goes out.", light);
                        ch.Act(ActOptions.ToCharacter, "{0} flickers and goes out.", light);
                        ItemManager.RemoveItem(light);
                    }
                    else if (!light.IsInfinite && light.TimeLeft < 5)
                        ch.Act(ActOptions.ToCharacter, "{0} flickers.", light);
                }

                // Update conditions
                pc?.GainCondition(Conditions.Drunk, -1); // decrease drunk state
                // TODO: not if undead from here
                pc?.GainCondition(Conditions.Full, ch.Size > Sizes.Medium ? -4 : -2);
                pc?.GainCondition(Conditions.Thirst, -1);
                pc?.GainCondition(Conditions.Hunger, ch.Size > Sizes.Medium ? -2 : -1);

                // apply a random periodic affect if any
                var periodicAuras = ch.Auras.Where(x => x.Affects.Any(a => a is ICharacterPeriodicAffect)).ToArray();
                if (periodicAuras.Length > 0)
                {
                    var periodicAura = periodicAuras.Random(RandomManager);
                    if (periodicAura != null)
                    {
                        var affect = periodicAura.Affects.OfType<ICharacterPeriodicAffect>().FirstOrDefault();
                        affect?.Apply(periodicAura, ch);
                    }
                }

                // TODO: limbo
                // TODO: autosave, autoquit
            }
            catch (Exception ex)
            { 
                Logger.LogError("Exception while handling character {name}. Exception: {ex}", ch.DebugName, ex);
            }
        }
    }

    private void HandleNonPlayableCharacters(int pulseCount)
    {
        // non-charmies, valid, in a room and is mob update always or area not empty
        var npcs = CharacterManager.NonPlayableCharacters.Where(x =>
                x.IsValid
                && x.Room != null
                && !x.CharacterFlags.IsSet("Charm")
                && (x.ActFlags.IsSet("UpdateAlways") || x.Room.Area.PlayableCharacters.Any())).ToArray(); // clone because special behavior could kill character and then modify list
        foreach (var npc in npcs)
        {
            try
            {
                // special behavior ?
                if (npc.SpecialBehavior != null)
                {
                    bool executed = npc.SpecialBehavior.Execute(npc);
                    if (executed)
                        continue;
                }

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
                    var mostValuable = npc.Room.Content.Where(x => !x.NoTake && x.Cost > 0 /*&& npc.CanLoot(item)*/).OrderByDescending(x => x.Cost).FirstOrDefault();
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
            catch (Exception ex)
            {
                Logger.LogError("Exception while handling npc {name}. Exception: {ex}", npc.DebugName, ex);
            }
        }
    }

    private void HandleItems(int pulseCount)
    {
        var decayingItems = ItemManager.Items.Where(x => x.DecayPulseLeft > 0).ToArray(); // clone bause decaying item will be removed from list
        foreach (var decayingItem in decayingItems)
        {
            try
            {
                //Logger.LogDebug($"HandleItems {item.DebugName} with {item.DecayPulseLeft} pulse left");
                decayingItem.DecreaseDecayPulseLeft(pulseCount);
                if (decayingItem.DecayPulseLeft == 0)
                {
                    Logger.LogDebug("Item {name} decays", decayingItem.DebugName);
                    var msg = "{0:N} crumbles into dust.";
                    switch (decayingItem)
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
                            if (decayingItem.WearLocation == WearLocations.Float)
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
                    if (decayingItem.ContainedInto is ICharacter wasOnCharacter)
                        wasOnCharacter.Act(ActOptions.ToCharacter, msg, decayingItem);
                    else if (decayingItem.ContainedInto is IRoom wasInRoom)
                    {
                        foreach (var character in wasInRoom.People)
                            character.Act(ActOptions.ToCharacter, msg, decayingItem);
                    }

                    // If container or playable character corpse, move items to contained into (except quest item)
                    if (decayingItem is IContainer container) // container
                    {
                        bool moveItems = !(decayingItem is IItemCorpse itemCorpse && !itemCorpse.IsPlayableCharacterCorpse); // don't perform if NPC corpse
                        if (moveItems)
                        {
                            Logger.LogDebug("Move item content to room");
                            var newContainer = decayingItem.ContainedInto;
                            if (newContainer == null)
                                Logger.LogError("Item was in the void");
                            else
                            {
                                var decayingContainerContent = container.Content.Where(x => x is not IItemQuest).ToArray(); // except quest item
                                foreach (var itemInContainer in decayingContainerContent)
                                    itemInContainer.ChangeContainer(newContainer);
                            }
                        }
                    }
                    ItemManager.RemoveItem(decayingItem);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception while handling item {name}. Exception: {ex}", decayingItem.DebugName, ex);
            }
        }

        // handle quest items respawn (only for predefined quests)
        var questsWithFloorItemQuestObjective = Players
            .Where(x => x.Impersonating != null)
            .Select(x => x.Impersonating!)
            .SelectMany(x => x.Quests.OfType<IPredefinedQuest>().Where(x => x.Objectives.OfType<FloorItemQuestObjective>().Any(y => !y.IsCompleted)))
            .DistinctBy(x => x.Blueprint.Id)
            .ToArray();
        foreach (var quest in questsWithFloorItemQuestObjective)
        {
            try
            {
                quest.SpawnQuestItemOnFloorIfNeeded();
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception while handling spawn of quest item {name} [id: {id}]. Exception: {ex}", quest.Title, quest.Blueprint.Id, ex);
            }
        }

        // handle quest items despawn (only for predefined quests)
        var questItemBlueprintIdsFoundInQuestObjectives = questsWithFloorItemQuestObjective.SelectMany(x => x.Objectives).OfType<FloorItemQuestObjective>().Select(x => x.ItemBlueprint.Id).Distinct().ToArray();
        var questItemsFoundOnFloor = RoomManager.Rooms.SelectMany(x => x.Content).OfType<IItemQuest>().ToArray(); // TODO: check everywhere ? ItemManager.Items.OfType<IItemQuest> instead ?
        var questItemsToRemove = questItemsFoundOnFloor.Where(x => !questItemBlueprintIdsFoundInQuestObjectives.Contains(x.Blueprint.Id)).ToArray();
        foreach (var questItemToRemove in questItemsToRemove)
        {
            try
            {
                Logger.LogInformation("Despawn quest item {name} contained into {room} because it's not found anymore on any active quest", questItemToRemove.DebugName, questItemToRemove.ContainedInto.DebugName);
                ItemManager.RemoveItem(questItemToRemove);
            }
            catch (Exception ex)
            {
                Logger.LogError("Exception while handling despawn of quest item {name}. Exception: {ex}", questItemToRemove.DebugName, ex);
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
            foreach (var character in CharacterManager.PlayableCharacters.Where(x => 
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
        PulseManager.Add("Resources", Pulse.PulsePerSeconds, Pulse.PulsePerSeconds, HandleResources);
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
            Stopwatch sw = new();
            while (true)
            {
                tickStopwatch.Restart();
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    Logger.LogInformation("Stop GameLoopTask requested");
                    break;
                }

                TimeManager.FixCurrentTime();

                // process inputs
                var inputElapsed = MonitorAction(sw, ProcessInput);

                // shutdown ongoing ?
                HandleShutdown();

                // trigger pulses
                var pulseElapsed = MonitorAction(sw, PulseManager.Pulse);

                // aggressive NPC
                // TODO: move HandleAggressiveNonPlayableCharacters to PulseManager with InitialValue and ResetValue 1 (4 times a second)
                var aggressiveNpcElapsed = MonitorAction(sw, HandleAggressiveNonPlayableCharacters);
                // daze&wait
                // TODO: move HandleDazeAndWait to PulseManager with InitialValue and ResetValue 1 (4 times a second)
                var dazeAndWaitElapsed = MonitorAction(sw, HandleDazeAndWait);

                // process outputs
                var outputElapsed = MonitorAction(sw, ProcessOutput);

                // cleanup
                var cleanupElapsed = MonitorAction(sw, Cleanup);

                tickStopwatch.Stop();

                // calculate wait time to reach 250ms for the loop, then wait
                long elapsedMs = tickStopwatch.ElapsedMilliseconds; // in milliseconds
                if (elapsedMs < Pulse.PulseDelay)
                {
                    Logger.LogTrace("Input ms: {inputElapsed}", inputElapsed);
                    Logger.LogTrace("pulse ms: {pulseElapsed}", pulseElapsed);
                    Logger.LogTrace("aggressive npc ms: {aggressiveNpcElapsed}", aggressiveNpcElapsed);
                    Logger.LogTrace("output ms: {outputElapsed}", outputElapsed);
                    Logger.LogTrace("cleanup ms: {cleanupElapsed}", cleanupElapsed);

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

    private static long MonitorAction(Stopwatch sw, Action action)
    {
        sw.Restart();
        action();
        sw.Stop();
        return sw.ElapsedMilliseconds;
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
