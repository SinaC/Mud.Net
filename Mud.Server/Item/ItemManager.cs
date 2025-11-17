using Mud.Domain;
using Mud.Logger;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.Table;
using Mud.Server.Random;
using Mud.Settings.Interfaces;
using System.Collections.ObjectModel;

namespace Mud.Server.Item;

public class ItemManager : IItemManager
{
    private IServiceProvider ServiceProvider { get; }
    private IGameActionManager GameActionManager { get; }
    private IAbilityManager AbilityManager { get; }
    private ISettings Settings { get; }
    private IRoomManager RoomManager { get; }
    private IAuraManager AuraManager { get; }
    private IRandomManager RandomManager { get; }
    private ITableValues TableValues { get; }

    private readonly Dictionary<int, ItemBlueprintBase> _itemBlueprints;
    private readonly List<IItem> _items;

    public ItemManager(IServiceProvider serviceProvider, IGameActionManager gameActionManager, IAbilityManager abilityManager, ISettings settings, IRoomManager roomManager, IAuraManager auraManager, IRandomManager randomManager, ITableValues tableValues)
    {
        ServiceProvider = serviceProvider;
        GameActionManager = gameActionManager;
        AbilityManager = abilityManager;
        Settings = settings;
        RoomManager = roomManager;
        AuraManager = auraManager;
        RandomManager = randomManager;
        TableValues = tableValues;

        _itemBlueprints = [];
        _items = [];
    }

    public IReadOnlyCollection<ItemBlueprintBase> ItemBlueprints
        => _itemBlueprints.Values.ToList().AsReadOnly();

    public ItemBlueprintBase? GetItemBlueprint(int id)
    {
        _itemBlueprints.TryGetValue(id, out var blueprint);
        return blueprint;
    }

    public TBlueprint? GetItemBlueprint<TBlueprint>(int id)
        where TBlueprint : ItemBlueprintBase
        => GetItemBlueprint(id) as TBlueprint;

    public void AddItemBlueprint(ItemBlueprintBase blueprint)
    {
        if (_itemBlueprints.ContainsKey(blueprint.Id))
            Log.Default.WriteLine(LogLevels.Error, "Item blueprint duplicate {0}!!!", blueprint.Id);
        else
            _itemBlueprints.Add(blueprint.Id, blueprint);
    }

    public IEnumerable<IItem> Items => _items.Where(x => x.IsValid);

    public IItemCorpse AddItemCorpse(Guid guid, IRoom room, ICharacter victim)
    {
        var blueprint = GetItemBlueprint<ItemCorpseBlueprint>(Settings.CorpseBlueprintId) ?? throw new Exception($"Corpse blueprint {Settings.CorpseBlueprintId} not found");
        var item = new ItemCorpse(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, RandomManager, this, guid, blueprint, room, victim);
        _items.Add(item);
        item.Recompute();
        return item;
    }

    public IItemCorpse AddItemCorpse(Guid guid, IRoom room, ICharacter victim, ICharacter killer)
    {
        var blueprint = GetItemBlueprint<ItemCorpseBlueprint>(Settings.CorpseBlueprintId) ?? throw new Exception($"Corpse blueprint {Settings.CorpseBlueprintId} not found");
        var item = new ItemCorpse(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, RandomManager, this, guid, blueprint, room, victim, killer);
        _items.Add(item);
        item.Recompute();
        return item;
    }

    public IItemMoney? AddItemMoney(Guid guid, long silverCoins, long goldCoins, IContainer container)
    {
        silverCoins = Math.Max(0, silverCoins);
        goldCoins = Math.Max(0, goldCoins);
        if (silverCoins == 0 && goldCoins == 0)
        {
            Log.Default.WriteLine(LogLevels.Error, "World.AddItemMoney: 0 silver and 0 gold.");
            return null;
        }
        int blueprintId = Settings.CoinsBlueprintId;
        var blueprint = GetItemBlueprint<ItemMoneyBlueprint>(blueprintId) ?? throw new Exception($"Money blueprint {blueprintId} not found");
        var money = new ItemMoney(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, blueprint, silverCoins, goldCoins, container);
        _items.Add(money);
        return money;
    }

    public IItem? AddItem(Guid guid, ItemBlueprintBase blueprint, IContainer container)
    {
        IItem? item = null;

        switch (blueprint)
        {
            case ItemArmorBlueprint armorBlueprint:
                item = new ItemArmor(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, armorBlueprint, container);
                break;
            case ItemBoatBlueprint boatBlueprint:
                item = new ItemBoat(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, boatBlueprint, container);
                break;
            case ItemClothingBlueprint clothingBlueprint:
                item = new ItemClothing(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, clothingBlueprint, container);
                break;
            case ItemContainerBlueprint containerBlueprint:
                item = new ItemContainer(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, this, guid, containerBlueprint, container);
                break;
            case ItemCorpseBlueprint corpseBlueprint:
                item = new ItemCorpse(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, RandomManager, this, guid, corpseBlueprint, container);
                break;
            case ItemDrinkContainerBlueprint drinkContainerBlueprint:
                item = new ItemDrinkContainer(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, drinkContainerBlueprint, container);
                break;
            case ItemFoodBlueprint foodBlueprint:
                item = new ItemFood(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, foodBlueprint, container);
                break;
            case ItemFurnitureBlueprint furnitureBlueprint:
                item = new ItemFurniture(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, furnitureBlueprint, container);
                break;
            case ItemFountainBlueprint fountainBlueprint:
                item = new ItemFountain(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, fountainBlueprint, container);
                break;
            case ItemGemBlueprint gemBlueprint:
                item = new ItemGem(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, gemBlueprint, container);
                break;
            case ItemJewelryBlueprint jewelryBlueprint:
                item = new ItemJewelry(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, jewelryBlueprint, container);
                break;
            case ItemJukeboxBlueprint jukeboxBlueprint:
                item = new ItemJukebox(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, jukeboxBlueprint, container);
                break;
            case ItemKeyBlueprint keyBlueprint:
                item = new ItemKey(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, keyBlueprint, container);
                break;
            case ItemLightBlueprint lightBlueprint:
                item = new ItemLight(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, lightBlueprint, container);
                break;
            case ItemMapBlueprint mapBlueprint:
                item = new ItemMap(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, mapBlueprint, container);
                break;
            case ItemMoneyBlueprint moneyBlueprint:
                item = new ItemMoney(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, moneyBlueprint, container);
                break;
            case ItemPillBlueprint pillBlueprint:
                item = new ItemPill(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, pillBlueprint, container);
                break;
            case ItemPotionBlueprint potionBlueprint:
                item = new ItemPotion(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, potionBlueprint, container);
                break;
            case ItemPortalBlueprint portalBlueprint:
                {
                    IRoom? destination = null;
                    if (portalBlueprint.Destination != ItemPortal.NoDestinationRoomId)
                    {
                        destination = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint?.Id == portalBlueprint.Destination);
                        if (destination == null)
                        {
                            destination = RoomManager.DefaultRecallRoom;
                            Log.Default.WriteLine(LogLevels.Error, "World.AddItem: PortalBlueprint {0} unknown destination {1} setting to recall {2}", blueprint.Id, portalBlueprint.Destination, Settings.DefaultRecallRoomId);
                        }
                    }
                    item = new ItemPortal(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, portalBlueprint, destination!, container);
                    break;
                }
            case ItemQuestBlueprint questBlueprint:
                item = new ItemQuest(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, questBlueprint, container);
                break;
            case ItemScrollBlueprint scrollBlueprint:
                item = new ItemScroll(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, scrollBlueprint, container);
                break;
            case ItemShieldBlueprint shieldBlueprint:
                item = new ItemShield(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, shieldBlueprint, container);
                break;
            case ItemStaffBlueprint staffBlueprint:
                item = new ItemStaff(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, staffBlueprint, container);
                break;
            case ItemTrashBlueprint trashBlueprint:
                item = new ItemTrash(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, trashBlueprint, container);
                break;
            case ItemTreasureBlueprint treasureBlueprint:
                item = new ItemTreasure(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, treasureBlueprint, container);
                break;
            case ItemWandBlueprint wandBlueprint:
                item = new ItemWand(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, wandBlueprint, container);
                break;
            case ItemWarpStoneBlueprint warpstoneBlueprint:
                item = new ItemWarpstone(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, warpstoneBlueprint, container);
                break;
            case ItemWeaponBlueprint weaponBlueprint:
                item = new ItemWeapon(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, TableValues, guid, weaponBlueprint, container);
                break;
            default:
                Log.Default.WriteLine(LogLevels.Error, "World.AddItem: unknown Item blueprint type {0}.", blueprint.GetType());
                break;
        }
        if (item != null)
        {
            item.Recompute();
            _items.Add(item);
            return item;
        }

        Log.Default.WriteLine(LogLevels.Error, "World.AddItem: unknown Item blueprint type {0}.", blueprint.GetType());
        return null;
    }

    public IItem? AddItem(Guid guid, ItemData itemData, IContainer container)
    {
        var blueprint = GetItemBlueprint(itemData.ItemId);
        if (blueprint == null)
        {
            Log.Default.WriteLine(LogLevels.Error, "World.AddItem: Item blueprint Id {0} doesn't exist anymore.", itemData.ItemId);
            return null;
        }

        IItem? item = null;
        switch (blueprint)
        {
            case ItemArmorBlueprint armorBlueprint:
                item = new ItemArmor(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, armorBlueprint, itemData, container); // no specific ItemData
                break;
            case ItemBoatBlueprint boatBlueprint:
                item = new ItemBoat(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, boatBlueprint, itemData, container);
                break;
            case ItemClothingBlueprint clothingBlueprint:
                item = new ItemClothing(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, clothingBlueprint, itemData, container);
                break;
            case ItemContainerBlueprint containerBlueprint:
                item = new ItemContainer(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, this, guid, containerBlueprint, itemData as ItemContainerData, container);
                break;
            case ItemCorpseBlueprint corpseBlueprint:
                item = new ItemCorpse(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, RandomManager, this, guid, corpseBlueprint, itemData as ItemCorpseData, container);
                break;
            case ItemDrinkContainerBlueprint drinkContainerBlueprint:
                item = new ItemDrinkContainer(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, drinkContainerBlueprint, itemData as ItemDrinkContainerData, container);
                break;
            case ItemFoodBlueprint foodBlueprint:
                item = new ItemFood(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, foodBlueprint, itemData as ItemFoodData, container);
                break;
            case ItemFurnitureBlueprint furnitureBlueprint:
                item = new ItemFurniture(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, furnitureBlueprint, itemData, container);
                break;
            case ItemFountainBlueprint fountainBlueprint:
                item = new ItemFountain(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, fountainBlueprint, itemData, container);
                break;
            case ItemGemBlueprint gemBlueprint:
                item = new ItemGem(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, gemBlueprint, itemData, container);
                break;
            case ItemJewelryBlueprint jewelryBlueprint:
                item = new ItemJewelry(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, jewelryBlueprint, itemData, container);
                break;
            case ItemJukeboxBlueprint jukeboxBlueprint:
                item = new ItemJukebox(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, jukeboxBlueprint, itemData, container);
                break;
            case ItemKeyBlueprint keyBlueprint:
                item = new ItemKey(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, keyBlueprint, itemData, container);
                break;
            case ItemLightBlueprint lightBlueprint:
                item = new ItemLight(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, lightBlueprint, itemData as ItemLightData, container);
                break;
            case ItemMapBlueprint mapBlueprint:
                item = new ItemMap(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, mapBlueprint, itemData, container);
                break;
            case ItemMoneyBlueprint moneyBlueprint:
                item = new ItemMoney(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, moneyBlueprint, itemData, container);
                break;
            case ItemPillBlueprint pillBlueprint:
                item = new ItemPill(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, pillBlueprint, itemData, container);
                break;
            case ItemPotionBlueprint potionBlueprint:
                item = new ItemPotion(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, potionBlueprint, itemData, container);
                break;
            case ItemPortalBlueprint portalBlueprint:
                {
                    var itemPortalData = itemData as ItemPortalData;
                    IRoom? destination = null;
                    if (itemPortalData != null && itemPortalData.DestinationRoomId != ItemPortal.NoDestinationRoomId)
                    {
                        destination = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint?.Id == itemPortalData.DestinationRoomId);
                        if (destination == null)
                        {
                            destination = RoomManager.DefaultRecallRoom;
                            Log.Default.WriteLine(LogLevels.Error, "World.AddItem: ItemPortalData unknown destination {0} setting to recall {1}", itemPortalData.DestinationRoomId, Settings.DefaultRecallRoomId);
                        }
                    }
                    item = new ItemPortal(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, portalBlueprint, itemPortalData!, destination!, container);
                }
                break;
            case ItemQuestBlueprint questBlueprint:
                item = new ItemQuest(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, questBlueprint, itemData, container);
                break;
            case ItemScrollBlueprint scrollBlueprint:
                item = new ItemScroll(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, scrollBlueprint, itemData, container);
                break;
            case ItemShieldBlueprint shieldBlueprint:
                item = new ItemShield(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, shieldBlueprint, itemData, container);
                break;
            case ItemStaffBlueprint staffBlueprint:
                item = new ItemStaff(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, staffBlueprint, itemData as ItemStaffData, container);
                break;
            case ItemTrashBlueprint trashBlueprint:
                item = new ItemTrash(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, trashBlueprint, itemData, container);
                break;
            case ItemTreasureBlueprint treasureBlueprint:
                item = new ItemTreasure(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, treasureBlueprint, itemData, container);
                break;
            case ItemWandBlueprint wandBlueprint:
                item = new ItemWand(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, wandBlueprint, itemData as ItemWandData, container);
                break;
            case ItemWarpStoneBlueprint warpstoneBlueprint:
                item = new ItemWarpstone(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, guid, warpstoneBlueprint, itemData, container);
                break;
            case ItemWeaponBlueprint weaponBlueprint:
                item = new ItemWeapon(ServiceProvider, GameActionManager, AbilityManager, Settings, RoomManager, AuraManager, TableValues, guid, weaponBlueprint, itemData as ItemWeaponData, container);
                break;
            default:
                Log.Default.WriteLine(LogLevels.Error, "World.AddItem: Unknown Item blueprint type {0}", blueprint.GetType());
                break;
        }

        if (item != null)
        {
            item.Recompute();
            _items.Add(item);
            return item;
        }
        Log.Default.WriteLine(LogLevels.Error, "World.AddItem: Invalid blueprint type  {0}", blueprint.GetType());
        return null;
    }

    public IItem? AddItem(Guid guid, int blueprintId, IContainer container)
    {
        var blueprint = GetItemBlueprint(blueprintId);
        if (blueprint == null)
        {
            Log.Default.WriteLine(LogLevels.Error, "World.AddItem: Item blueprint Id {0} doesn't exist anymore.", blueprintId);
            return null;
        }
        var item = AddItem(guid, blueprint, container);
        if (item == null)
            Log.Default.WriteLine(LogLevels.Error, "World.AddItem: Unknown blueprint id {0} or type {1}.", blueprintId, blueprint.GetType().FullName ?? "???");
        return item;
    }

    public void RemoveItem(IItem item)
    {
        item.ChangeContainer(RoomManager.NullRoom); // move to NullRoom
        item.ChangeEquippedBy(null, false);
        // If container, remove content
        if (item is IContainer container)
        {
            var content = new ReadOnlyCollection<IItem>(container.Content.ToList()); // clone to be sure
            foreach (var itemInContainer in content)
                RemoveItem(itemInContainer);
        }
        // Remove auras
        var auras = new ReadOnlyCollection<IAura>(item.Auras.ToList()); // clone
        foreach (var aura in auras)
        {
            aura.OnRemoved();
            item.RemoveAura(aura, false);
        }
        // no need to recompute
        //
        item.OnRemoved();
        //_items.Remove(item); will be removed in cleanup step
    }

    public void Cleanup()
    {
        if (_items.Any(x => !x.IsValid))
            Log.Default.WriteLine(LogLevels.Debug, $"Cleaning {_items.Count(x => !x.IsValid)} item(s)");

        var itemsToRemove = _items.Where(x => !x.IsValid).ToArray();
        foreach (var item in itemsToRemove)
            item.OnCleaned(); // definitive remove
        _items.RemoveAll(x => !x.IsValid);
    }
}
