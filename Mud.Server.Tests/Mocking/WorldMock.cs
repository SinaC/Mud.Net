﻿using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Mud.Domain;
using Mud.Server.Blueprints.Area;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.LootTable;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Blueprints.Room;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.World;
using Mud.Server.Item;

namespace Mud.Server.Tests.Mocking
{
    internal class WorldMock : IWorld
    {
        private readonly List<ICharacter> _characters;
        private readonly List<IRoom> _rooms;
        private readonly List<IItem> _items;
        private readonly List<IArea> _areas;
        private readonly List<QuestBlueprint> _questBlueprints;
        private readonly List<AreaBlueprint> _areaBlueprints;
        private readonly List<RoomBlueprint> _roomBlueprints;
        private readonly List<CharacterBlueprintBase> _characterBlueprints;
        private readonly List<ItemBlueprintBase> _itemBlueprints;

        public WorldMock()
        {
            _characters = new List<ICharacter>();
            _rooms = new List<IRoom>();
            _items = new List<IItem>();
            _areas = new List<IArea>();
            _questBlueprints = new List<QuestBlueprint>();
            _areaBlueprints = new List<AreaBlueprint>();
            _roomBlueprints = new List<RoomBlueprint>();
            _characterBlueprints = new List<CharacterBlueprintBase>();
            _itemBlueprints = new List<ItemBlueprintBase>();
        }

        public void Clear()
        {
            _characters.Clear();
            _rooms.Clear();
            _items.Clear();
            _areas.Clear();
            _questBlueprints.Clear();
            _roomBlueprints.Clear();
            _characterBlueprints.Clear();
            _itemBlueprints.Clear();
            _areaBlueprints.Clear();
        }

        #region IWorld

        public IReadOnlyCollection<TreasureTable<int>> TreasureTables { get; }
        public void AddTreasureTable(TreasureTable<int> table)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<QuestBlueprint> QuestBlueprints => _questBlueprints;
        public IReadOnlyCollection<AreaBlueprint> AreaBlueprints => _areaBlueprints;
        public IReadOnlyCollection<RoomBlueprint> RoomBlueprints => _roomBlueprints;
        public IReadOnlyCollection<CharacterBlueprintBase> CharacterBlueprints => _characterBlueprints;
        public IReadOnlyCollection<ItemBlueprintBase> ItemBlueprints => _itemBlueprints;

        public QuestBlueprint GetQuestBlueprint(int id)
        {
            return _questBlueprints.FirstOrDefault(x => x.Id == id);
        }

        public AreaBlueprint GetAreaBlueprint(int id)
        {
            throw new NotImplementedException();
        }

        public RoomBlueprint GetRoomBlueprint(int id)
        {
            return _roomBlueprints.FirstOrDefault(x => x.Id == id);
        }

        public CharacterBlueprintBase GetCharacterBlueprint(int id)
        {
            return _characterBlueprints.FirstOrDefault(x => x.Id == id);
        }

        public ItemBlueprintBase GetItemBlueprint(int id)
        {
            return _itemBlueprints.FirstOrDefault(x => x.Id == id);
        }

        public TBlueprint GetCharacterBlueprint<TBlueprint>(int id)
            where TBlueprint: CharacterBlueprintBase
        {
            return _characterBlueprints.OfType<TBlueprint>().FirstOrDefault(x => x.Id == id);
        }

        public TBlueprint GetItemBlueprint<TBlueprint>(int id)
            where TBlueprint: ItemBlueprintBase
        {
            return _itemBlueprints.OfType<TBlueprint>().FirstOrDefault(x => x.Id == id);
        }

        public void AddQuestBlueprint(QuestBlueprint blueprint)
        {
            _questBlueprints.Add(blueprint);
        }

        public void AddAreaBlueprint(AreaBlueprint blueprint)
        {
            throw new NotImplementedException();
        }

        public void AddRoomBlueprint(RoomBlueprint blueprint)
        {
            _roomBlueprints.Add(blueprint);
        }

        public void AddCharacterBlueprint(CharacterBlueprintBase blueprint)
        {
            _characterBlueprints.Add(blueprint);
        }

        public void AddItemBlueprint(ItemBlueprintBase blueprint)
        {
            _itemBlueprints.Add(blueprint);
        }

        public IEnumerable<IArea> Areas => _areas;

        public IEnumerable<IRoom> Rooms => _rooms;

        public IEnumerable<ICharacter> Characters => _characters;
        public IEnumerable<INonPlayableCharacter> NonPlayableCharacters => Characters.OfType<INonPlayableCharacter>();
        public IEnumerable<IPlayableCharacter> PlayableCharacters => Characters.OfType<IPlayableCharacter>();

        public IEnumerable<IItem> Items => _items;

        public IRoom NullRoom => new Mock<IRoom>().Object;

        public IRoom DefaultRecallRoom => new Mock<IRoom>().Object;

        public IRoom DefaultDeathRoom => new Mock<IRoom>().Object;

        public IRoom GetRandomRoom(ICharacter character)
        {
            throw new NotImplementedException();
        }

        public IArea AddArea(Guid guid, AreaBlueprint blueprint)
        {
            throw new NotImplementedException();
        }

        public IRoom AddRoom(Guid guid, RoomBlueprint blueprint, IArea area)
        {
            IRoom room = new Room.Room(guid, blueprint, area);
            _rooms.Add(room);
            return room;
        }

        public IPlayableCharacter AddPlayableCharacter(Guid guid, PlayableCharacterData characterData, IPlayer player, IRoom room)
        {
            IPlayableCharacter character = new Character.PlayableCharacter.PlayableCharacter(guid, characterData, player, room);
            _characters.Add(character);
            return character;
        }

        public INonPlayableCharacter AddNonPlayableCharacter(Guid guid, CharacterBlueprintBase blueprint, IRoom room)
        {
            INonPlayableCharacter character = new Character.NonPlayableCharacter.NonPlayableCharacter(guid, blueprint, room);
            _characters.Add(character);
            return character;
        }

        public INonPlayableCharacter AddNonPlayableCharacter(Guid guid, CharacterBlueprintBase blueprint, PetData petData, IRoom room)
        {
            throw new NotImplementedException();
        }

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
                        IRoom destination = Rooms.FirstOrDefault(x => x.Blueprint?.Id == portalBlueprint.Destination);
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
                        IRoom destination = Rooms.FirstOrDefault(x => x.Blueprint?.Id == portalBlueprint.Destination);
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
                _items.Add(item);
                return item;
            }

            return null;
        }

        public IItem AddItem(Guid guid, int blueprintId, IContainer container)
        {
            throw new NotImplementedException();
        }

        public IAura AddAura(IEntity target, IAbility ability, IEntity source, int level, TimeSpan ts, AuraFlags auraFlags, bool recompute, params IAffect[] affects)
        {
            throw new NotImplementedException();
        }

        public void FixWorld()
        {
            throw new NotImplementedException();
        }

        //public IPeriodicAura AddPeriodicAura(IEntity entity, IAbility ability, IEntity source, int amount, AmountOperators amountOperator, int level, bool tickVisible, int tickDelay, int totalTicks)
        //{
        //    throw new NotImplementedException();
        //}

        //public IPeriodicAura AddPeriodicAura(IEntity entity, IAbility ability, IEntity source, SchoolTypes school, int amount, AmountOperators amountOperator, int level, bool tickVisible, int tickDelay, int totalTicks)
        //{
        //    throw new NotImplementedException();
        //}

        public void RemoveCharacter(ICharacter character)
        {
            throw new NotImplementedException();
        }

        public void RemoveItem(IItem item)
        {
            throw new NotImplementedException();
        }

        public void RemoveRoom(IRoom room)
        {
            throw new NotImplementedException();
        }

        public IExit AddExit(IRoom from, IRoom to, ExitBlueprint blueprint, ExitDirections direction)
        {
            throw new NotImplementedException();
        }

        public void Cleanup()
        {
            throw new NotImplementedException();
        }

        public void ResetWorld()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
