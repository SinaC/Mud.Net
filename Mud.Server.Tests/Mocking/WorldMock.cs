using System;
using System.Collections.Generic;
using Mud.Server.Blueprints;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.LootTable;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Blueprints.Room;
using Mud.Server.Constants;
using Mud.Server.Item;

namespace Mud.Server.Tests.Mocking
{
    public class WorldMock : IWorld
    {
        private readonly List<ICharacter> _characters;
        private readonly List<IRoom> _rooms;
        private readonly List<IItem> _items;

        public WorldMock()
        {
            _characters = new List<ICharacter>();
            _rooms = new List<IRoom>();
            _items = new List<IItem>();
        }

        #region IWorld

        public IReadOnlyCollection<TreasureTable<int>> TreasureTables { get; }
        public void AddTreasureTable(TreasureTable<int> table)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<QuestBlueprint> QuestBlueprints { get; }
        public IReadOnlyCollection<RoomBlueprint> RoomBlueprints { get; }
        public IReadOnlyCollection<CharacterBlueprint> CharacterBlueprints { get; }
        public IReadOnlyCollection<ItemBlueprintBase> ItemBlueprints { get; }

        public QuestBlueprint GetQuestBlueprint(int id)
        {
            throw new NotImplementedException();
        }

        public RoomBlueprint GetRoomBlueprint(int id)
        {
            throw new NotImplementedException();
        }

        public CharacterBlueprint GetCharacterBlueprint(int id)
        {
            throw new NotImplementedException();
        }

        public ItemBlueprintBase GetItemBlueprint(int id)
        {
            throw new NotImplementedException();
        }

        public void AddQuestBlueprint(QuestBlueprint blueprint)
        {
            throw new NotImplementedException();
        }

        public void AddRoomBlueprint(RoomBlueprint blueprint)
        {
            throw new NotImplementedException();
        }

        public void AddCharacterBlueprint(CharacterBlueprint blueprint)
        {
            throw new NotImplementedException();
        }

        public void AddItemBlueprint(ItemBlueprintBase blueprint)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IArea> Areas { get; }

        public IEnumerable<IRoom> Rooms => _rooms;

        public IEnumerable<ICharacter> Characters => _characters;

        public IEnumerable<IItem> Items => _items;

        public IArea AddArea(Guid guid, string displayName, int minLevel, int maxLevel, string builders, string credits)
        {
            throw new NotImplementedException();
        }

        public IRoom AddRoom(Guid guid, RoomBlueprint blueprint, IArea area)
        {
            throw new NotImplementedException();
        }

        public ICharacter AddCharacter(Guid guid, string name, IClass pcClass, IRace pcRace, Sex pcSex, IRoom room)
        {
            throw new NotImplementedException();
        }

        public ICharacter AddCharacter(Guid guid, CharacterBlueprint blueprint, IRoom room)
        {
            throw new NotImplementedException();
        }

        public IItemContainer AddItemContainer(Guid guid, ItemContainerBlueprint blueprint, IContainer container)
        {
            throw new NotImplementedException();
        }

        public IItemArmor AddItemArmor(Guid guid, ItemArmorBlueprint blueprint, IContainer container)
        {
            throw new NotImplementedException();
        }

        public IItemWeapon AddItemWeapon(Guid guid, ItemWeaponBlueprint blueprint, IContainer container)
        {
            throw new NotImplementedException();
        }

        public IItemLight AddItemLight(Guid guid, ItemLightBlueprint blueprint, IContainer container)
        {
            throw new NotImplementedException();
        }

        public IItemCorpse AddItemCorpse(Guid guid, ItemCorpseBlueprint blueprint, IRoom container, ICharacter victim, ICharacter killer)
        {
            throw new NotImplementedException();
        }

        public IItemShield AddItemShield(Guid guid, ItemShieldBlueprint blueprint, IContainer container)
        {
            throw new NotImplementedException();
        }

        public IItemFurniture AddItemFurniture(Guid guid, ItemFurnitureBlueprint blueprint, IContainer container)
        {
            throw new NotImplementedException();
        }

        public IItemJewelry AddItemJewelry(Guid guid, ItemJewelryBlueprint blueprint, IContainer container)
        {
            throw new NotImplementedException();
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
