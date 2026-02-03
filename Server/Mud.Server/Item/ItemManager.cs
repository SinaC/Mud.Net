using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Item;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Domain.Attributes;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;
using System.Reflection;

namespace Mud.Server.Item;

[Export(typeof(IItemManager)), Shared]
public class ItemManager : IItemManager
{
    private ILogger<ItemManager> Logger { get; }
    private IServiceProvider ServiceProvider { get; }
    private IRoomManager RoomManager { get; }
    private IFlagsManager FlagsManager { get; }
    private int CorpseBlueprintId { get; }
    private int CoinsBlueprintId { get; }

    private readonly Dictionary<Type, ItemDefinition> _itemDefinitionByBlueprintType;
    private readonly Dictionary<int, ItemBlueprintBase> _itemBlueprints;
    private readonly List<IItem> _items;
    private readonly Dictionary<int, int> _instanceCountByBlueprintId;

    public ItemManager(ILogger<ItemManager> logger, IServiceProvider serviceProvider, IAssemblyHelper assemblyHelper, IOptions<WorldOptions> worldOptions, IRoomManager roomManager, IFlagsManager flagsManager)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;
        RoomManager = roomManager;
        FlagsManager = flagsManager;
        CorpseBlueprintId = worldOptions.Value.BlueprintIds.Corpse;
        CoinsBlueprintId = worldOptions.Value.BlueprintIds.Coins;

        var iItemType = typeof(IItem);
        var itemDataType = typeof(ItemData);
        var itemDefinitions = new List<ItemDefinition>();
        // use Item attribute to generate ItemDefinitions
        //      ItemType, ItemBlueprintType, ItemDataType
        //      InitializeWithoutItemDataMethod: search for one of the following methods
        //          Initialize(Guid, SpecificItemBlueprint, string, IContainer)
        //       or Initialize<SpecificBlueprint>(Guid, SpecificItemBlueprint, string, IContainer)
        //      InitializeWithItemDataMethod: search for one of the following methods
        //          Initialize(Guid, SpecificItemBlueprint, SpecificItemData, IContainer)
        //          Initialize<SpecificBlueprint,SpecificItemData>(Guid, SpecificItemBlueprint, SpecificItemData, IContainer)
        foreach (var itemType in assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(iItemType))))
        {
            var itemAttribute = itemType.GetCustomAttribute<ItemAttribute>() ?? throw new Exception($"ItemManager: no ItemAttribute found for Item {itemType.FullName}");
            var isItemDataTypeValid = itemAttribute.ItemDataType.IsAssignableTo(itemDataType);
            if (!isItemDataTypeValid)
                throw new Exception($"ItemManager: ItemData type {itemAttribute.ItemDataType} doesn't inherit from {itemDataType.FullName} on Item {itemType.FullName}");
            var initializeWithoutItemDataMethod =
                itemType.SearchMethod("Initialize", [typeof(Guid), itemAttribute.BlueprintType, typeof(string), typeof(IContainer)])
                ?? itemType.SearchMethod("Initialize", [typeof(ItemBlueprintBase)], [typeof(Guid), itemAttribute.BlueprintType, typeof(string), typeof(IContainer)])
                ?? throw new Exception($"ItemManager: no valid Initialize with blueprint method found on Item {itemType.FullName}");
            var initializeWithItemDataMethod =
                itemType.SearchMethod("Initialize", [typeof(Guid), itemAttribute.BlueprintType, itemAttribute.ItemDataType, typeof(IContainer)])
                ?? itemType.SearchMethod("Initialize", [typeof(ItemBlueprintBase), typeof(ItemData)], [typeof(Guid), itemAttribute.BlueprintType, itemAttribute.ItemDataType, typeof(IContainer)])
                ?? throw new Exception($"ItemManager: no valid Initialize with blueprint and itemdata method found on Item {itemType.FullName}");
            var itemDefinition = new ItemDefinition(itemType, itemAttribute.BlueprintType, itemAttribute.ItemDataType, initializeWithoutItemDataMethod, initializeWithItemDataMethod);
            itemDefinitions.Add(itemDefinition);
        }
        _itemDefinitionByBlueprintType = itemDefinitions.ToDictionary(x => x.BlueprintType);

        _itemBlueprints = [];
        _items = [];
        _instanceCountByBlueprintId = [];
    }

    public IReadOnlyCollection<ItemBlueprintBase> ItemBlueprints
        => _itemBlueprints.Values.ToList().AsReadOnly();

    public ItemBlueprintBase? GetItemBlueprint(int id)
        => _itemBlueprints.GetValueOrDefault(id);

    public TBlueprint? GetItemBlueprint<TBlueprint>(int id)
        where TBlueprint : ItemBlueprintBase
        => GetItemBlueprint(id) as TBlueprint;

    public void AddItemBlueprint(ItemBlueprintBase blueprint)
    {
        if (!_itemBlueprints.TryAdd(blueprint.Id, blueprint))
            Logger.LogError("ItemManager: item blueprint duplicate {blueprintId}!!!", blueprint.Id);
        else
        {
            if (!FlagsManager.CheckFlags(blueprint.ItemFlags))
                Logger.LogError("ItemManager: item blueprint {blueprintId} has invalid flags", blueprint.Id);
        }
    }

    public IEnumerable<IItem> Items => _items.Where(x => x.IsValid);

    public int Count(int blueprintId) => _instanceCountByBlueprintId.GetValueOrDefault(blueprintId);

    public IItem? AddItem(Guid guid, ItemBlueprintBase blueprint, string source, IContainer container)
    {
        if (!_itemBlueprints.ContainsKey(blueprint.Id))
        {
            Logger.LogError("ItemManager: item blueprint {blueprintId} doesn't exist", blueprint.Id);
            return null;
        }

        // create and initialize item
        var blueprintType = blueprint.GetType();
        if (!_itemDefinitionByBlueprintType.TryGetValue(blueprintType, out var itemDefinition))
        {
            Logger.LogError("ItemManager: unexpected Blueprint type {blueprintType}.", blueprintType.FullName);
            return null;
        }
        var item = CreateInstance(itemDefinition);
        if (item == null)
        {
            Logger.LogError("ItemManager: cannot create instance of Item {itemType}", itemDefinition.ItemType.FullName);
            return null;
        }
        try
        {
            itemDefinition.InitializeWithoutItemDataMethod.Invoke(item, [guid, blueprint, source, container]);
        }
        catch (Exception ex)
        {
            Logger.LogError("ItemManager: cannot initialize item {itemType} blueprint Id {blueprintId} Exception: {ex}.", itemDefinition.ItemType.FullName, blueprint.Id, ex);
            return null;
        }
        // add item to collection
        if (item.Name == null || item.Id == Guid.Empty)
        {
            Logger.LogError("ItemManager: item initialization failed for blueprint Id {blueprintId}.", blueprint.Id);
            return null;
        }
        item.Recompute();
        if (!FlagsManager.CheckFlags(blueprint.ItemFlags))
            Logger.LogError("ItemManager: item blueprint {blueprintId} has invalid flags", blueprint.Id);
        _items.Add(item);
        _instanceCountByBlueprintId.Increment(blueprint.Id);
        return item;
    }

    public IItem? AddItem(Guid guid, ItemData itemData, IContainer container)
    {
        // get blueprint
        var blueprint = GetItemBlueprint(itemData.ItemId);
        if (blueprint == null)
        {
            Logger.LogError("ItemManager: Item blueprint Id {blueprintId} doesn't exist anymore.", itemData.ItemId);
            return null;
        }
        // create and initialize item
        var blueprintType = blueprint.GetType();
        if (!_itemDefinitionByBlueprintType.TryGetValue(blueprintType, out var itemDefinition))
        {
            Logger.LogError("ItemManager: unexpected Blueprint type {blueprintType}.", blueprintType.FullName);
            return null;
        }
        var item = CreateInstance(itemDefinition);
        if (item == null)
        {
            Logger.LogError("ItemManager: cannot create instance of Item {itemType}", itemDefinition.ItemType.FullName);
            return null;
        }
        try
        {
            itemDefinition.InitializeWithItemDataMethod.Invoke(item, [guid, blueprint, itemData, container]);
        }
        catch (Exception ex)
        {
            Logger.LogError("ItemManager: cannot initialize item {itemType} blueprint Id {blueprintId} item data type {itemDataType} Exception: {ex}.", itemDefinition.ItemType.FullName, blueprint.Id, itemDefinition.ItemDataType.GetType().FullName, ex);
            return null;
        }
        // add item to collection
        if (item.Name == null || item.Id == Guid.Empty)
        {
            Logger.LogError("ItemManager: item initialization failed for blueprint Id {blueprintId}.", blueprint.Id);
            return null;
        }
        item.Recompute();
        if (!FlagsManager.CheckFlags(blueprint.ItemFlags))
            Logger.LogError("ItemManager: item blueprint {blueprintId} has invalid flags", blueprint.Id);
        _items.Add(item);
        _instanceCountByBlueprintId.Increment(blueprint.Id);
        return item;
    }

    public IItem? AddItem(Guid guid, int blueprintId, string source, IContainer container)
    {
        var blueprint = GetItemBlueprint(blueprintId);
        if (blueprint == null)
        {
            Logger.LogError("ItemManager: Item blueprint Id {blueprintId} doesn't exist anymore.", blueprintId);
            return null;
        }
        var item = AddItem(guid, blueprint, source, container);
        if (item == null)
            Logger.LogError("ItemManager: Unknown blueprint id {blueprintId} or type {blueprintType}.", blueprintId, blueprint.GetType().FullName ?? "???");
        return item;
    }

    public TItem? AddItem<TItem>(Guid guid, int blueprintId, string source, IContainer container)
        where TItem : class, IItem
    {
        var blueprint = GetItemBlueprint(blueprintId);
        if (blueprint == null)
        {
            Logger.LogError("ItemManager: Item blueprint Id {blueprintId} doesn't exist anymore.", blueprintId);
            return null;
        }
        var instance = AddItem(guid, blueprint, source, container);
        if (instance == null)
        {
            Logger.LogError("ItemManager: Unknown blueprint id {blueprintId} or type {blueprintType}.", blueprintId, blueprint.GetType().FullName ?? "???");
            return null;
        }
        var item = instance as TItem;
        if (item == null)
            Logger.LogError("ItemManager: trying to cast item blueprint id {blueprintId} type {blueprintType} to item type {itemType}.", blueprintId, blueprint.GetType().FullName ?? "???", typeof(TItem).FullName);
        return item;
    }

    public IItemCorpse? AddItemCorpse(Guid guid, ICharacter victim, string source, IRoom room)
    {
        var blueprint = GetItemBlueprint<ItemCorpseBlueprint>(CorpseBlueprintId);
        if (blueprint == null)
        {
            Logger.LogError("ItemCorpseBlueprint (id:{corpseBlueprintId}) doesn't exist !!!", CorpseBlueprintId);
            return null;
        }
        var corpse = ServiceProvider.GetRequiredService<ItemCorpse>();
        corpse.Initialize(guid, blueprint, victim, source, room);
        if (!FlagsManager.CheckFlags(blueprint.ItemFlags))
            Logger.LogError("ItemManager: corpse blueprint {blueprintId} has invalid flags", blueprint.Id);
        _items.Add(corpse);
        _instanceCountByBlueprintId.Increment(blueprint.Id);
        return corpse;
    }

    public IItemMoney? AddItemMoney(Guid guid, long silverCoins, long goldCoins, string source, IContainer container)
    {
        silverCoins = Math.Max(0, silverCoins);
        goldCoins = Math.Max(0, goldCoins);
        if (silverCoins == 0 && goldCoins == 0)
        {
            Logger.LogError("AddItemMoney: 0 silver and 0 gold.");
            return null;
        }
        var blueprint = GetItemBlueprint<ItemMoneyBlueprint>(CoinsBlueprintId);
        if (blueprint == null)
        {
            Logger.LogError("ItemManager: itemMoneyBlueprint (id:{coinsBlueprintId}) doesn't exist !!!", CoinsBlueprintId);
            return null;
        }

        var money = ServiceProvider.GetRequiredService<ItemMoney>();
        money.Initialize(guid, blueprint, silverCoins, goldCoins, source, container);
        money.Recompute();
        if (!FlagsManager.CheckFlags(blueprint.ItemFlags))
            Logger.LogError("ItemManager: money blueprint {blueprintId} has invalid flags", blueprint.Id);
        _items.Add(money);
        _instanceCountByBlueprintId.Increment(blueprint.Id);
        return money;
    }

    public void RemoveItem(IItem item)
    {
        item.ChangeContainer(RoomManager.NullRoom); // move to NullRoom
        item.ChangeEquippedBy(null, false);
        // If container, remove content
        if (item is IContainer container)
        {
            var content = container.Content.ToArray(); // clone to be sure
            foreach (var itemInContainer in content)
                RemoveItem(itemInContainer);
        }
        // Remove auras
        var auras = item.Auras.ToArray(); // clone
        foreach (var aura in auras)
        {
            item.RemoveAura(aura, false, false);
        }
        // no need to recompute
        //
        var containedInto = item.ContainedInto;
        item.OnRemoved(RoomManager.NullRoom);
        containedInto?.Recompute();

        //_items.Remove(item); will be removed in cleanup step
        _instanceCountByBlueprintId.Decrement(item.Blueprint.Id);
    }

    public void Cleanup()
    {
        if (_items.Any(x => !x.IsValid))
            Logger.LogDebug("Cleaning {count} item(s)", _items.Count(x => !x.IsValid));

        var itemsToRemove = _items.Where(x => !x.IsValid).ToArray();
        foreach (var item in itemsToRemove)
            item.OnCleaned(); // definitive remove
        _items.RemoveAll(x => !x.IsValid);
    }

    private IItem? CreateInstance(ItemDefinition itemDefinition)
    {
        var item = ServiceProvider.GetRequiredService(itemDefinition.ItemType);
        //var item = Activator.CreateInstance(itemDefinition.ItemType); cannot be used because ItemXXX dont have parameterless ctor
        if (item is not IItem instance)
        {
            Logger.LogError("ItemManager: item {itemType} cannot be created or is not of type {expectedItemType}", itemDefinition.ItemType.FullName ?? "???", typeof(IItem).FullName ?? "???");
            return null;
        }
        return instance;
    }

    private class ItemDefinition
    {
        public Type ItemType { get; }
        public Type BlueprintType { get; }
        public Type ItemDataType { get; }
        public MethodInfo InitializeWithoutItemDataMethod { get; }
        public MethodInfo InitializeWithItemDataMethod { get; }

        public ItemDefinition(Type itemType, Type blueprintType, Type itemDataType, MethodInfo initializeWithoutItemDataMethod, MethodInfo initializeWithItemDataMethod)
        {
            ItemType = itemType;
            BlueprintType = blueprintType;
            ItemDataType = itemDataType;
            InitializeWithoutItemDataMethod = initializeWithoutItemDataMethod;
            InitializeWithItemDataMethod = initializeWithItemDataMethod;
        }
    }
}
