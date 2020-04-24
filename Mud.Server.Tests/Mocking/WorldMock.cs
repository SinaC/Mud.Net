using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Domain;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.LootTable;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Blueprints.Room;
using Mud.Server.Item;

namespace Mud.Server.Tests.Mocking
{
    internal class WorldMock : IWorld
    {
        private readonly List<ICharacter> _characters;
        private readonly List<IRoom> _rooms;
        private readonly List<IItem> _items;
        private readonly List<QuestBlueprint> _questBlueprints;
        private readonly List<RoomBlueprint> _roomBlueprints;
        private readonly List<CharacterBlueprintBase> _characterBlueprints;
        private readonly List<ItemBlueprintBase> _itemBlueprints;

        public WorldMock()
        {
            _characters = new List<ICharacter>();
            _rooms = new List<IRoom>();
            _items = new List<IItem>();
            _questBlueprints = new List<QuestBlueprint>();
            _roomBlueprints = new List<RoomBlueprint>();
            _characterBlueprints = new List<CharacterBlueprintBase>();
            _itemBlueprints = new List<ItemBlueprintBase>();
        }

        public void Clear()
        {
            _characters.Clear();
            _rooms.Clear();
            _items.Clear();
            _questBlueprints.Clear();
            _roomBlueprints.Clear();
            _characterBlueprints.Clear();
            _itemBlueprints.Clear();
        }

        #region IWorld

        public IReadOnlyCollection<TreasureTable<int>> TreasureTables { get; }
        public void AddTreasureTable(TreasureTable<int> table)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<QuestBlueprint> QuestBlueprints => _questBlueprints;
        public IReadOnlyCollection<RoomBlueprint> RoomBlueprints => _roomBlueprints;
        public IReadOnlyCollection<CharacterBlueprintBase> CharacterBlueprints => _characterBlueprints;
        public IReadOnlyCollection<ItemBlueprintBase> ItemBlueprints => _itemBlueprints;

        public QuestBlueprint GetQuestBlueprint(int id)
        {
            return _questBlueprints.FirstOrDefault(x => x.Id == id);
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

        public IEnumerable<IArea> Areas { get; }

        public IEnumerable<IRoom> Rooms => _rooms;

        public IEnumerable<ICharacter> Characters => _characters;
        public IEnumerable<INonPlayableCharacter> NonPlayableCharacters => Characters.OfType<INonPlayableCharacter>();
        public IEnumerable<IPlayableCharacter> PlayableCharacters => Characters.OfType<IPlayableCharacter>();

        public IEnumerable<IItem> Items => _items;

        public IArea AddArea(Guid guid, string displayName, int minLevel, int maxLevel, string builders, string credits)
        {
            throw new NotImplementedException();
        }

        public IRoom AddRoom(Guid guid, RoomBlueprint blueprint, IArea area)
        {
            IRoom room = new Room.Room(guid, blueprint, area);
            _rooms.Add(room);
            return room;
        }

        public IPlayableCharacter AddPlayableCharacter(Guid guid, CharacterData characterData, IPlayer player, IRoom room)
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

        public IItemContainer AddItemContainer(Guid guid, ItemContainerBlueprint blueprint, IContainer container)
        {
            IItemContainer item = new ItemContainer(guid, blueprint, container);
            _items.Add(item);
            return item;
        }

        public IItemArmor AddItemArmor(Guid guid, ItemArmorBlueprint blueprint, IContainer container)
        {
            IItemArmor item = new ItemArmor(guid, blueprint, container);
            _items.Add(item);
            return item;
        }

        public IItemWeapon AddItemWeapon(Guid guid, ItemWeaponBlueprint blueprint, IContainer container)
        {
            IItemWeapon item = new ItemWeapon(guid, blueprint, container);
            _items.Add(item);
            return item;
        }

        public IItemLight AddItemLight(Guid guid, ItemLightBlueprint blueprint, IContainer container)
        {
            IItemLight itemLight = new ItemLight(guid, blueprint, container);
            _items.Add(itemLight);
            return itemLight;
        }

        public IItemCorpse AddItemCorpse(Guid guid, ItemCorpseBlueprint blueprint, IRoom room, ICharacter victim)
        {
            IItemCorpse itemCorpse = new ItemCorpse(guid, blueprint, room, victim);
            _items.Add(itemCorpse);
            return itemCorpse;
        }

        public IItemCorpse AddItemCorpse(Guid guid, ItemCorpseBlueprint blueprint, IRoom container, ICharacter victim, ICharacter killer)
        {
            throw new NotImplementedException();
        }

        public IItemShield AddItemShield(Guid guid, ItemShieldBlueprint blueprint, IContainer container)
        {
            IItemShield item = new ItemShield(guid, blueprint, container);
            _items.Add(item);
            return item;
        }

        public IItemFurniture AddItemFurniture(Guid guid, ItemFurnitureBlueprint blueprint, IContainer container)
        {
            IItemFurniture item = new ItemFurniture(guid, blueprint, container);
            _items.Add(item);
            return item;
        }

        public IItemJewelry AddItemJewelry(Guid guid, ItemJewelryBlueprint blueprint, IContainer container)
        {
            IItemJewelry item = new ItemJewelry(guid, blueprint, container);
            _items.Add(item);
            return item;
        }

        public IItemQuest AddItemQuest(Guid guid, ItemQuestBlueprint blueprint, IContainer container)
        {
            IItemQuest item = new ItemQuest(guid, blueprint, container);
            _items.Add(item);
            return item;
        }

        public IItemKey AddItemKey(Guid guid, ItemKeyBlueprint blueprint, IContainer container)
        {
            IItemKey item = new ItemKey(guid, blueprint, container);
            _items.Add(item);
            return item;
        }

        public IItemPortal AddItemPortal(Guid guid, ItemPortalBlueprint blueprint, IRoom destination, IContainer container)
        {
            IItemPortal item = new ItemPortal(guid, blueprint, destination, container);
            _items.Add(item);
            return item;
        }

        public IItem AddItem(Guid guid, ItemBlueprintBase blueprint, IContainer container)
        {
            throw new NotImplementedException();
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
                case ItemContainerBlueprint containerBlueprint:
                    item = new ItemContainer(guid, containerBlueprint, itemData as ItemContainerData, container);
                    break;
                case ItemCorpseBlueprint corpseBlueprint:
                    item = new ItemCorpse(guid, corpseBlueprint, itemData as ItemCorpseData, container);
                    break;
                case ItemFurnitureBlueprint furnitureBlueprint:
                    item = new ItemFurniture(guid, furnitureBlueprint, itemData, container);
                    break;
                case ItemJewelryBlueprint jewelryBlueprint:
                    item = new ItemJewelry(guid, jewelryBlueprint, itemData, container);
                    break;
                case ItemKeyBlueprint keyBlueprint:
                    item = new ItemKey(guid, keyBlueprint, itemData, container);
                    break;
                case ItemLightBlueprint lightBlueprint:
                    item = new ItemLight(guid, lightBlueprint, itemData, container);
                    break;
                case ItemPortalBlueprint portalBlueprint:
                    {
                        IRoom destination = Rooms.FirstOrDefault(x => x.Blueprint?.Id == portalBlueprint.Destination);
                        item = new ItemPortal(guid, portalBlueprint, itemData, destination, container);
                    }
                    break;
                case ItemQuestBlueprint questBlueprint:
                    item = new ItemQuest(guid, questBlueprint, itemData, container);
                    break;
                case ItemShieldBlueprint shieldBlueprint:
                    item = new ItemShield(guid, shieldBlueprint, itemData, container);
                    break;
                case ItemWeaponBlueprint weaponBlueprint:
                    item = new ItemWeapon(guid, weaponBlueprint, itemData, container);
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

        public IAura AddAura(ICharacter victim, IAbility ability, ICharacter source, AuraModifiers modifier, int amount, AmountOperators amountOperator, int totalSeconds, bool visible)
        {
            throw new NotImplementedException();
        }

        public IPeriodicAura AddPeriodicAura(ICharacter victim, IAbility ability, ICharacter source, int amount, AmountOperators amountOperator, bool tickVisible, int tickDelay, int totalTicks)
        {
            throw new NotImplementedException();
        }

        public IPeriodicAura AddPeriodicAura(ICharacter victim, IAbility ability, ICharacter source, SchoolTypes school, int amount, AmountOperators amountOperator, bool tickVisible, int tickDelay, int totalTicks)
        {
            throw new NotImplementedException();
        }

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

        public IItem AddItemContainer(Guid guid, ItemBlueprintBase blueprint, IContainer container)
        {
            throw new NotImplementedException();
        }

        public IItem AddItemArmor(Guid guid, ItemBlueprintBase blueprint, IContainer container)
        {
            throw new NotImplementedException();
        }

        public IItem AddItemWeapon(Guid guid, ItemBlueprintBase blueprint, IContainer container)
        {
            throw new NotImplementedException();
        }

        public IItem AddItemLight(Guid guid, ItemBlueprintBase blueprint, IContainer container)
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            // TODO
        }

        public void Cleanup()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
