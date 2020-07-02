using Mud.Container;
using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Item;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mud.Server.Tests.Mocking
{
    internal class ItemManagerMock : IItemManager
    {
        private IRoomManager RoomManager => DependencyContainer.Current.GetInstance<IRoomManager>();

        private readonly List<ItemBlueprintBase> _itemBlueprints = new List<ItemBlueprintBase>();

        private readonly List<IItem> _items = new List<IItem>();

        public IEnumerable<IItem> Items => _items;

        public IReadOnlyCollection<ItemBlueprintBase> ItemBlueprints => _itemBlueprints;

        public IItemCorpse AddItemCorpse(Guid guid, IRoom room, ICharacter victim)
        {
            throw new NotImplementedException();
        }

        public IItemCorpse AddItemCorpse(Guid guid, IRoom container, ICharacter victim, ICharacter killer)
        {
            throw new NotImplementedException();
        }

        public IItemMoney AddItemMoney(Guid guid, long silverCoins, long goldCoins, IContainer container)
        {
            throw new NotImplementedException();
        }

        public IItem AddItem(Guid guid, ItemBlueprintBase blueprint, IContainer container)
        {
            IItem item = null;
            switch (blueprint)
            {
                case ItemArmorBlueprint armorBlueprint:
                    item = new ItemArmor(guid, armorBlueprint, container); // no specific ItemData
                    break;
                case ItemBoatBlueprint boatBlueprint:
                    item = new ItemBoat(guid, boatBlueprint, container);
                    break;
                case ItemClothingBlueprint clothingBlueprint:
                    item = new ItemClothing(guid, clothingBlueprint, container);
                    break;
                case ItemContainerBlueprint containerBlueprint:
                    item = new ItemContainer(guid, containerBlueprint, container);
                    break;
                case ItemCorpseBlueprint corpseBlueprint:
                    item = new ItemCorpse(guid, corpseBlueprint, container);
                    break;
                case ItemFurnitureBlueprint furnitureBlueprint:
                    item = new ItemFurniture(guid, furnitureBlueprint, container);
                    break;
                case ItemFountainBlueprint fountainBlueprint:
                    item = new ItemFountain(guid, fountainBlueprint, container);
                    break;
                case ItemDrinkContainerBlueprint drinkContainerBlueprint:
                    item = new ItemDrinkContainer(guid, drinkContainerBlueprint, container);
                    break;
                case ItemFoodBlueprint foodBlueprint:
                    item = new ItemFood(guid, foodBlueprint, container);
                    break;
                case ItemGemBlueprint gemBlueprint:
                    item = new ItemGem(guid, gemBlueprint, container);
                    break;
                case ItemJewelryBlueprint jewelryBlueprint:
                    item = new ItemJewelry(guid, jewelryBlueprint, container);
                    break;
                case ItemJukeboxBlueprint jukeboxBlueprint:
                    item = new ItemJukebox(guid, jukeboxBlueprint, container);
                    break;
                case ItemKeyBlueprint keyBlueprint:
                    item = new ItemKey(guid, keyBlueprint, container);
                    break;
                case ItemLightBlueprint lightBlueprint:
                    item = new ItemLight(guid, lightBlueprint, container);
                    break;
                case ItemMapBlueprint mapBlueprint:
                    item = new ItemMap(guid, mapBlueprint, container);
                    break;
                case ItemPillBlueprint pillBlueprint:
                    item = new ItemPill(guid, pillBlueprint, container);
                    break;
                case ItemPotionBlueprint potionBlueprint:
                    item = new ItemPotion(guid, potionBlueprint, container);
                    break;
                case ItemPortalBlueprint portalBlueprint:
                    {
                        IRoom destination = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint?.Id == portalBlueprint.Destination);
                        item = new ItemPortal(guid, portalBlueprint, destination, container);
                    }
                    break;
                case ItemTrashBlueprint trashBlueprint:
                    item = new ItemTrash(guid, trashBlueprint, container);
                    break;
                case ItemQuestBlueprint questBlueprint:
                    item = new ItemQuest(guid, questBlueprint, container);
                    break;
                case ItemScrollBlueprint scrollBlueprint:
                    item = new ItemScroll(guid, scrollBlueprint, container);
                    break;
                case ItemShieldBlueprint shieldBlueprint:
                    item = new ItemShield(guid, shieldBlueprint, container);
                    break;
                case ItemStaffBlueprint staffBlueprint:
                    item = new ItemStaff(guid, staffBlueprint, container);
                    break;
                case ItemWandBlueprint wandBlueprint:
                    item = new ItemWand(guid, wandBlueprint, container);
                    break;
                case ItemWeaponBlueprint weaponBlueprint:
                    item = new ItemWeapon(guid, weaponBlueprint, container);
                    break;
                case ItemWarpStoneBlueprint warpstoneBlueprint:
                    item = new ItemWarpstone(guid, warpstoneBlueprint, container);
                    break;
            }

            if (item != null)
            {
                _items.Add(item);
                return item;
            }

            return null;
        }

        public IItem AddItem(Guid guid, ItemData itemData, IContainer container) // almost same method in real implementation
        {
            ItemBlueprintBase blueprint = GetItemBlueprint(itemData.ItemId);
            if (blueprint == null)
                return null;

            Debug.Print($"Blueprint({blueprint.Name}).WearLoc: {blueprint.WearLocation}");

            IItem item = null;
            switch (blueprint)
            {
                case ItemArmorBlueprint armorBlueprint:
                    item = new ItemArmor(guid, armorBlueprint, itemData, container); // no specific ItemData
                    break;
                case ItemBoatBlueprint boatBlueprint:
                    item = new ItemBoat(guid, boatBlueprint, itemData, container);
                    break;
                case ItemClothingBlueprint clothingBlueprint:
                    item = new ItemClothing(guid, clothingBlueprint, itemData, container);
                    break;
                case ItemContainerBlueprint containerBlueprint:
                    item = new ItemContainer(guid, containerBlueprint, itemData as ItemContainerData, container);
                    break;
                case ItemCorpseBlueprint corpseBlueprint:
                    item = new ItemCorpse(guid, corpseBlueprint, itemData as ItemCorpseData, container);
                    break;
                case ItemFurnitureBlueprint furnitureBlueprint:
                    item = new ItemFurniture(guid, furnitureBlueprint, itemData, container);
                    break;
                case ItemFountainBlueprint fountainBlueprint:
                    item = new ItemFountain(guid, fountainBlueprint, itemData, container);
                    break;
                case ItemDrinkContainerBlueprint drinkContainerBlueprint:
                    item = new ItemDrinkContainer(guid, drinkContainerBlueprint, itemData as ItemDrinkContainerData, container);
                    break;
                case ItemFoodBlueprint foodBlueprint:
                    item = new ItemFood(guid, foodBlueprint, itemData as ItemFoodData, container);
                    break;
                case ItemGemBlueprint gemBlueprint:
                    item = new ItemGem(guid, gemBlueprint, itemData, container);
                    break;
                case ItemJewelryBlueprint jewelryBlueprint:
                    item = new ItemJewelry(guid, jewelryBlueprint, itemData, container);
                    break;
                case ItemJukeboxBlueprint jukeboxBlueprint:
                    item = new ItemJukebox(guid, jukeboxBlueprint, itemData, container);
                    break;
                case ItemKeyBlueprint keyBlueprint:
                    item = new ItemKey(guid, keyBlueprint, itemData, container);
                    break;
                case ItemLightBlueprint lightBlueprint:
                    item = new ItemLight(guid, lightBlueprint, itemData as ItemLightData, container);
                    break;
                case ItemMapBlueprint mapBlueprint:
                    item = new ItemMap(guid, mapBlueprint, itemData, container);
                    break;
                case ItemPillBlueprint pillBlueprint:
                    item = new ItemPill(guid, pillBlueprint, itemData, container);
                    break;
                case ItemPotionBlueprint potionBlueprint:
                    item = new ItemPotion(guid, potionBlueprint, itemData, container);
                    break;
                case ItemPortalBlueprint portalBlueprint:
                    {
                        IRoom destination = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint?.Id == portalBlueprint.Destination);
                        item = new ItemPortal(guid, portalBlueprint, itemData as ItemPortalData, destination, container);
                    }
                    break;
                case ItemTrashBlueprint trashBlueprint:
                    item = new ItemTrash(guid, trashBlueprint, itemData, container);
                    break;
                case ItemQuestBlueprint questBlueprint:
                    item = new ItemQuest(guid, questBlueprint, itemData, container);
                    break;
                case ItemScrollBlueprint scrollBlueprint:
                    item = new ItemScroll(guid, scrollBlueprint, itemData, container);
                    break;
                case ItemShieldBlueprint shieldBlueprint:
                    item = new ItemShield(guid, shieldBlueprint, itemData, container);
                    break;
                case ItemStaffBlueprint staffBlueprint:
                    item = new ItemStaff(guid, staffBlueprint, itemData as ItemStaffData, container);
                    break;
                case ItemWandBlueprint wandBlueprint:
                    item = new ItemWand(guid, wandBlueprint, itemData as ItemWandData, container);
                    break;
                case ItemWeaponBlueprint weaponBlueprint:
                    item = new ItemWeapon(guid, weaponBlueprint, itemData as ItemWeaponData, container);
                    break;
                case ItemWarpStoneBlueprint warpstoneBlueprint:
                    item = new ItemWarpstone(guid, warpstoneBlueprint, container);
                    break;
            }

            if (item != null)
            {
                Debug.Print($"Item({item.Name}).WearLoc: {item.WearLocation}");
                _items.Add(item);
                return item;
            }

            return null;
        }

        public IItem AddItem(Guid guid, int blueprintId, IContainer container)
        {
            throw new NotImplementedException();
        }

        public ItemBlueprintBase GetItemBlueprint(int id) => _itemBlueprints.FirstOrDefault(x => x.Id == id);

        public TBlueprint GetItemBlueprint<TBlueprint>(int id) where TBlueprint : ItemBlueprintBase => _itemBlueprints.OfType<TBlueprint>().FirstOrDefault(x => x.Id == id);

        public void RemoveItem(IItem item)
        {
            throw new NotImplementedException();
        }

        public void AddItemBlueprint(ItemBlueprintBase blueprint)
        {
            _itemBlueprints.Add(blueprint);
        }

        public void Clear()
        {
            _items.Clear();
            _itemBlueprints.Clear();
        }
    }
}
