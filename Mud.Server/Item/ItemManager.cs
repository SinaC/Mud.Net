using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Blueprints.Room;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;
using System.Collections.ObjectModel;

namespace Mud.Server.Item;

public class ItemManager : IItemManager
{
    private ILogger<ItemManager> Logger { get; }
    private IServiceProvider ServiceProvider { get; }
    private IRoomManager RoomManager { get; }
    private int CorpseBlueprintId { get; }
    private int CoinsBlueprintId { get; }

    private readonly Dictionary<int, ItemBlueprintBase> _itemBlueprints;
    private readonly List<IItem> _items;

    public ItemManager(ILogger<ItemManager> logger, IServiceProvider serviceProvider, IOptions<WorldOptions> worldOptions, IRoomManager roomManager)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;
        RoomManager = roomManager;
        CorpseBlueprintId = worldOptions.Value.BlueprintIds.Corpse;
        CoinsBlueprintId = worldOptions.Value.BlueprintIds.Coins;

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
            Logger.LogError("Item blueprint duplicate {blueprintId}!!!", blueprint.Id);
        else
            _itemBlueprints.Add(blueprint.Id, blueprint);
    }

    public IEnumerable<IItem> Items => _items.Where(x => x.IsValid);

    public IItemCorpse? AddItemCorpse(Guid guid, IRoom room, ICharacter victim)
    {
        var blueprint = GetItemBlueprint<ItemCorpseBlueprint>(CorpseBlueprintId);
        if (blueprint == null)
        {
            Logger.LogError("ItemCorpseBlueprint (id:{corpseBlueprintId}) doesn't exist !!!", CorpseBlueprintId);
            return null;
        }
        var item = ServiceProvider.GetRequiredService<IItemCorpse>();
        item.Initialize(guid, blueprint, room, victim);
        _items.Add(item);
        item.Recompute();
        return item;
    }

    public IItemCorpse? AddItemCorpse(Guid guid, IRoom room, ICharacter victim, ICharacter killer)
    {
        var blueprint = GetItemBlueprint<ItemCorpseBlueprint>(CorpseBlueprintId);
        if (blueprint == null)
        {
            Logger.LogError("ItemCorpseBlueprint (id:{corpseBlueprintId}) doesn't exist !!!", CorpseBlueprintId);
            return null;
        }
        var item = ServiceProvider.GetRequiredService<IItemCorpse>();
        item.Initialize(guid, blueprint, room, victim, killer);
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
            Logger.LogError("World.AddItemMoney: 0 silver and 0 gold.");
            return null;
        }
        var blueprint = GetItemBlueprint<ItemMoneyBlueprint>(CoinsBlueprintId);
        if (blueprint == null)
        {
            Logger.LogError("ItemMoneyBlueprint (id:{coinsBlueprintId}) doesn't exist !!!", CoinsBlueprintId);
            return null;
        }

        var money = ServiceProvider.GetRequiredService<IItemMoney>();
        money.Initialize(guid, blueprint, silverCoins, goldCoins, container);
        _items.Add(money);
        return money;
    }

    public IItem? AddItem(Guid guid, ItemBlueprintBase blueprint, IContainer container)
    {
        IItem? item = null;
        switch (blueprint)
        {
            case ItemArmorBlueprint armorBlueprint:
                {
                    var armor = ServiceProvider.GetRequiredService<IItemArmor>();
                    armor.Initialize(guid, armorBlueprint, container);
                    item = armor;
                    break;
                }
            case ItemBoatBlueprint boatBlueprint:
                {
                    var boat = ServiceProvider.GetRequiredService<IItemBoat>();
                    boat.Initialize(guid, boatBlueprint, container);
                    item = boat;
                    break;
                }
            case ItemClothingBlueprint clothingBlueprint:
                {
                    var clothing = ServiceProvider.GetRequiredService<IItemClothing>();
                    clothing.Initialize(guid, clothingBlueprint, container);
                    item = clothing;
                    break;
                }
            case ItemContainerBlueprint containerBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemContainer>();
                    containerItem.Initialize(guid, containerBlueprint, container);
                    item = containerItem;
                    break;
                }
            case ItemCorpseBlueprint corpseBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemCorpse>();
                    containerItem.Initialize(guid, corpseBlueprint, container);
                    item = containerItem;
                    break;
                }
            case ItemDrinkContainerBlueprint drinkContainerBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemContainer>();
                    containerItem.Initialize(guid, drinkContainerBlueprint, container);
                    item = containerItem;
                    break;
                }
            case ItemFoodBlueprint foodBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemFood>();
                    containerItem.Initialize(guid, foodBlueprint, container);
                    item = containerItem;
                    break;
                }
            case ItemFurnitureBlueprint furnitureBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemFurniture>();
                    containerItem.Initialize(guid, furnitureBlueprint, container);
                    item = containerItem;
                    break;
                }
            case ItemFountainBlueprint fountainBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemFountain>();
                    containerItem.Initialize(guid, fountainBlueprint, container);
                    item = containerItem;
                    break;
                }
            case ItemGemBlueprint gemBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemGem>();
                    containerItem.Initialize(guid, gemBlueprint, container);
                    item = containerItem;
                    break;
                }
            case ItemJewelryBlueprint jewelryBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemJewelry>();
                    containerItem.Initialize(guid, jewelryBlueprint, container);
                    item = containerItem;
                    break;
                }
            case ItemJukeboxBlueprint jukeboxBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemJukebox>();
                    containerItem.Initialize(guid, jukeboxBlueprint, container);
                    item = containerItem;
                    break;
                }
            case ItemKeyBlueprint keyBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemKey>();
                    containerItem.Initialize(guid, keyBlueprint, container);
                    item = containerItem;
                    break;
                }
            case ItemLightBlueprint lightBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemLight>();
                    containerItem.Initialize(guid, lightBlueprint, container);
                    item = containerItem;
                    break;
                }
            case ItemMapBlueprint mapBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemMap>();
                    containerItem.Initialize(guid, mapBlueprint, container);
                    item = containerItem;
                    break;
                }
            case ItemMoneyBlueprint moneyBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemMoney>();
                    containerItem.Initialize(guid, moneyBlueprint, container);
                    item = containerItem;
                    break;
                }
            case ItemPillBlueprint pillBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemPill>();
                    containerItem.Initialize(guid, pillBlueprint, container);
                    item = containerItem;
                    break;
                }
            case ItemPotionBlueprint potionBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemPotion>();
                    containerItem.Initialize(guid, potionBlueprint, container);
                    item = containerItem;
                    break;
                }
            case ItemPortalBlueprint portalBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemPortal>();
                    containerItem.Initialize(guid, portalBlueprint, container);
                    item = containerItem;
                    break;
                }
            case ItemQuestBlueprint questBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemQuest>();
                    containerItem.Initialize(guid, questBlueprint, container);
                    item = containerItem;
                    break;
                }
            case ItemScrollBlueprint scrollBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemScroll>();
                    containerItem.Initialize(guid, scrollBlueprint, container);
                    item = containerItem;
                    break;
                }
            case ItemShieldBlueprint shieldBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemShield>();
                    containerItem.Initialize(guid, shieldBlueprint, container);
                    item = containerItem;
                    break;
                }
            case ItemStaffBlueprint staffBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemStaff>();
                    containerItem.Initialize(guid, staffBlueprint, container);
                    item = containerItem;
                    break;
                }
            case ItemTrashBlueprint trashBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemTrash>();
                    containerItem.Initialize(guid, trashBlueprint, container);
                    item = containerItem;
                    break;
                }
            case ItemTreasureBlueprint treasureBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemTreasure>();
                    containerItem.Initialize(guid, treasureBlueprint, container);
                    item = containerItem;
                    break;
                }
            case ItemWandBlueprint wandBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemWand>();
                    containerItem.Initialize(guid, wandBlueprint, container);
                    item = containerItem;
                    break;
                }
            case ItemWarpStoneBlueprint warpstoneBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemWarpstone>();
                    containerItem.Initialize(guid, warpstoneBlueprint, container);
                    item = containerItem;
                    break;
                }
            case ItemWeaponBlueprint weaponBlueprint:
                {
                    var weapon = ServiceProvider.GetRequiredService<IItemWeapon>();
                    weapon.Initialize(guid, weaponBlueprint, container);
                    item = weapon;
                    break;
                }
            default:
                Logger.LogError("World.AddItem: unknown Item blueprint type {blueprintType}.", blueprint.GetType());
                break;
        }
        if (item == null)
        {
            Logger.LogError("World.AddItem: unknown Item blueprint type {blueprintType}.", blueprint.GetType());
            return null;
        }
        if (item.Name == null || item.Id == Guid.Empty)
        {
            Logger.LogError("World.AddItem: item initialization failed for blueprint Id {blueprintId}.", blueprint.Id);
            return null;
        }

        item.Recompute();
        _items.Add(item);
        return item;
    }

    public IItem? AddItem(Guid guid, ItemData itemData, IContainer container)
    {
        var blueprint = GetItemBlueprint(itemData.ItemId);
        if (blueprint == null)
        {
            Logger.LogError("World.AddItem: Item blueprint Id {blueprintId} doesn't exist anymore.", itemData.ItemId);
            return null;
        }

        IItem? item = null;
        switch (blueprint)
        {
            case ItemArmorBlueprint armorBlueprint:
                {
                    var armor = ServiceProvider.GetRequiredService<IItemArmor>();
                    armor.Initialize(guid, armorBlueprint, itemData, container);
                    item = armor;
                    break;
                }
            case ItemBoatBlueprint boatBlueprint:
                {
                    var boat = ServiceProvider.GetRequiredService<IItemBoat>();
                    boat.Initialize(guid, boatBlueprint, itemData, container);
                    item = boat;
                    break;
                }
            case ItemClothingBlueprint clothingBlueprint:
                {
                    var clothing = ServiceProvider.GetRequiredService<IItemClothing>();
                    clothing.Initialize(guid, clothingBlueprint, itemData, container);
                    item = clothing;
                    break;
                }
            case ItemContainerBlueprint containerBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemContainer>();
                    containerItem.Initialize(guid, containerBlueprint, itemData as ItemContainerData, container);
                    item = containerItem;
                    break;
                }
            case ItemCorpseBlueprint corpseBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemCorpse>();
                    containerItem.Initialize(guid, corpseBlueprint, itemData as ItemCorpseData, container);
                    item = containerItem;
                    break;
                }
            case ItemDrinkContainerBlueprint drinkContainerBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemContainer>();
                    containerItem.Initialize(guid, drinkContainerBlueprint, itemData as ItemDrinkContainerData, container);
                    item = containerItem;
                    break;
                }
            case ItemFoodBlueprint foodBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemFood>();
                    containerItem.Initialize(guid, foodBlueprint, itemData as ItemFoodData, container);
                    item = containerItem;
                    break;
                }
            case ItemFurnitureBlueprint furnitureBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemFurniture>();
                    containerItem.Initialize(guid, furnitureBlueprint, itemData, container);
                    item = containerItem;
                    break;
                }
            case ItemFountainBlueprint fountainBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemFountain>();
                    containerItem.Initialize(guid, fountainBlueprint, itemData, container);
                    item = containerItem;
                    break;
                }
            case ItemGemBlueprint gemBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemGem>();
                    containerItem.Initialize(guid, gemBlueprint, itemData, container);
                    item = containerItem;
                    break;
                }
            case ItemJewelryBlueprint jewelryBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemJewelry>();
                    containerItem.Initialize(guid, jewelryBlueprint, itemData, container);
                    item = containerItem;
                    break;
                }
            case ItemJukeboxBlueprint jukeboxBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemJukebox>();
                    containerItem.Initialize(guid, jukeboxBlueprint, itemData, container);
                    item = containerItem;
                    break;
                }
            case ItemKeyBlueprint keyBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemKey>();
                    containerItem.Initialize(guid, keyBlueprint, itemData, container);
                    item = containerItem;
                    break;
                }
            case ItemLightBlueprint lightBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemLight>();
                    containerItem.Initialize(guid, lightBlueprint, itemData as ItemLightData, container);
                    item = containerItem;
                    break;
                }
            case ItemMapBlueprint mapBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemMap>();
                    containerItem.Initialize(guid, mapBlueprint, itemData, container);
                    item = containerItem;
                    break;
                }
            case ItemMoneyBlueprint moneyBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemMoney>();
                    containerItem.Initialize(guid, moneyBlueprint, itemData, container);
                    item = containerItem;
                    break;
                }
            case ItemPillBlueprint pillBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemPill>();
                    containerItem.Initialize(guid, pillBlueprint, itemData, container);
                    item = containerItem;
                    break;
                }
            case ItemPotionBlueprint potionBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemPotion>();
                    containerItem.Initialize(guid, potionBlueprint, itemData, container);
                    item = containerItem;
                    break;
                }
            case ItemPortalBlueprint portalBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemPortal>();
                    containerItem.Initialize(guid, portalBlueprint, itemData as ItemPortalData, container);
                    item = containerItem;
                    break;
                }
            case ItemQuestBlueprint questBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemQuest>();
                    containerItem.Initialize(guid, questBlueprint, itemData, container);
                    item = containerItem;
                    break;
                }
            case ItemScrollBlueprint scrollBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemScroll>();
                    containerItem.Initialize(guid, scrollBlueprint, itemData, container);
                    item = containerItem;
                    break;
                }
            case ItemShieldBlueprint shieldBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemShield>();
                    containerItem.Initialize(guid, shieldBlueprint, itemData, container);
                    item = containerItem;
                    break;
                }
            case ItemStaffBlueprint staffBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemStaff>();
                    containerItem.Initialize(guid, staffBlueprint, itemData as ItemStaffData, container);
                    item = containerItem;
                    break;
                }
            case ItemTrashBlueprint trashBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemTrash>();
                    containerItem.Initialize(guid, trashBlueprint, itemData, container);
                    item = containerItem;
                    break;
                }
            case ItemTreasureBlueprint treasureBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemTreasure>();
                    containerItem.Initialize(guid, treasureBlueprint, itemData, container);
                    item = containerItem;
                    break;
                }
            case ItemWandBlueprint wandBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemWand>();
                    containerItem.Initialize(guid, wandBlueprint, itemData as ItemWandData, container);
                    item = containerItem;
                    break;
                }
            case ItemWarpStoneBlueprint warpstoneBlueprint:
                {
                    var containerItem = ServiceProvider.GetRequiredService<IItemWarpstone>();
                    containerItem.Initialize(guid, warpstoneBlueprint, itemData, container);
                    item = containerItem;
                    break;
                }
            case ItemWeaponBlueprint weaponBlueprint:
                {
                    var weapon = ServiceProvider.GetRequiredService<IItemWeapon>();
                    weapon.Initialize(guid, weaponBlueprint, itemData as ItemWeaponData, container);
                    item = weapon;
                    break;
                }
            default:
                Logger.LogError("World.AddItem: unknown Item blueprint type {blueprintType}.", blueprint.GetType());
                break;
        }

        if (item == null)
        {
            Logger.LogError("World.AddItem: Invalid blueprint type  {blueprintType}", blueprint.GetType());
            return null;
        }
        if (item.Name == null || item.Id == Guid.Empty)
        {
            Logger.LogError("World.AddItem: item initialization failed for blueprint Id {blueprintId}.", blueprint.Id);
            return null;
        }

        item.Recompute();
        _items.Add(item);
        return item;
    }

    public IItem? AddItem(Guid guid, int blueprintId, IContainer container)
    {
        var blueprint = GetItemBlueprint(blueprintId);
        if (blueprint == null)
        {
            Logger.LogError("World.AddItem: Item blueprint Id {blueprintId} doesn't exist anymore.", blueprintId);
            return null;
        }
        var item = AddItem(guid, blueprint, container);
        if (item == null)
            Logger.LogError("World.AddItem: Unknown blueprint id {blueprintId} or type {blueprintType}.", blueprintId, blueprint.GetType().FullName ?? "???");
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
            Logger.LogDebug("Cleaning {count} item(s)", _items.Count(x => !x.IsValid));

        var itemsToRemove = _items.Where(x => !x.IsValid).ToArray();
        foreach (var item in itemsToRemove)
            item.OnCleaned(); // definitive remove
        _items.RemoveAll(x => !x.IsValid);
    }
}
