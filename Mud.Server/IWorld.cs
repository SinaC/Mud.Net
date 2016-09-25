using System;
using System.Collections.Generic;
using Mud.Datas.DataContracts;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.LootTable;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Blueprints.Room;
using Mud.Server.Constants;
using Mud.Server.Item;

namespace Mud.Server
{
    public interface IWorld
    {
        // Treasures
        IReadOnlyCollection<TreasureTable<int>> TreasureTables { get; }

        void AddTreasureTable(TreasureTable<int> table);

        // Blueprints
        IReadOnlyCollection<QuestBlueprint> QuestBlueprints { get; }
        IReadOnlyCollection<RoomBlueprint> RoomBlueprints { get; }
        IReadOnlyCollection<CharacterBlueprint> CharacterBlueprints { get; }
        IReadOnlyCollection<ItemBlueprintBase> ItemBlueprints { get; }

        QuestBlueprint GetQuestBlueprint(int id);
        RoomBlueprint GetRoomBlueprint(int id);
        CharacterBlueprint GetCharacterBlueprint(int id);
        ItemBlueprintBase GetItemBlueprint(int id);

        void AddQuestBlueprint(QuestBlueprint blueprint);
        void AddRoomBlueprint(RoomBlueprint blueprint);
        void AddCharacterBlueprint(CharacterBlueprint blueprint);
        void AddItemBlueprint(ItemBlueprintBase blueprint);

        //
        IEnumerable<IArea> Areas { get; }
        IEnumerable<IRoom> Rooms { get; }
        IEnumerable<ICharacter> Characters { get; }
        IEnumerable<IItem> Items { get; }

        IArea AddArea(Guid guid, string displayName, int minLevel, int maxLevel, string builders, string credits);

        IRoom AddRoom(Guid guid, RoomBlueprint blueprint, IArea area);

        IExit AddExit(IRoom from, IRoom to, ExitBlueprint blueprint, ExitDirections direction);

        ICharacter AddCharacter(Guid guid, CharacterData characterData, IRoom room); // Impersonable
        ICharacter AddCharacter(Guid guid, CharacterBlueprint blueprint, IRoom room); // Non-impersonable
        
        IItemContainer AddItemContainer(Guid guid, ItemContainerBlueprint blueprint, IContainer container);
        IItemArmor AddItemArmor(Guid guid, ItemArmorBlueprint blueprint, IContainer container);
        IItemWeapon AddItemWeapon(Guid guid, ItemWeaponBlueprint blueprint, IContainer container);
        IItemLight AddItemLight(Guid guid, ItemLightBlueprint blueprint, IContainer container);
        IItemCorpse AddItemCorpse(Guid guid, ItemCorpseBlueprint blueprint, IRoom room, ICharacter victim);
        IItemCorpse AddItemCorpse(Guid guid, ItemCorpseBlueprint blueprint, IRoom room, ICharacter victim, ICharacter killer);
        IItemShield AddItemShield(Guid guid, ItemShieldBlueprint blueprint, IContainer container);
        IItemFurniture AddItemFurniture(Guid guid, ItemFurnitureBlueprint blueprint, IContainer container);
        IItemJewelry AddItemJewelry(Guid guid, ItemJewelryBlueprint blueprint, IContainer container);
        IItemQuest AddItemQuest(Guid guid, ItemQuestBlueprint blueprint, IContainer container);
        IItemKey AddItemKey(Guid guid, ItemKeyBlueprint blueprint, IContainer container);
        IItemPortal AddItemPortal(Guid guid, ItemPortalBlueprint blueprint, IRoom destination, IContainer container);
        IItem AddItem(Guid guid, ItemBlueprintBase blueprint, IContainer container);
        IItem AddItem(Guid guid, int blueprintId, IContainer container);

        IAura AddAura(ICharacter victim, IAbility ability, ICharacter source, AuraModifiers modifier, int amount, AmountOperators amountOperator, int totalSeconds, bool recompute);
        IPeriodicAura AddPeriodicAura(ICharacter victim, IAbility ability, ICharacter source, int amount, AmountOperators amountOperator, bool tickVisible, int tickDelay, int totalTicks); // Hot
        IPeriodicAura AddPeriodicAura(ICharacter victim, IAbility ability, ICharacter source, SchoolTypes school, int amount, AmountOperators amountOperator, bool tickVisible, int tickDelay, int totalTicks); // Dot

        void RemoveCharacter(ICharacter character);
        void RemoveItem(IItem item);
        void RemoveRoom(IRoom room);

        void Update(); // called every pulse
        void Cleanup(); // called once outputs has been processed (before next loop)
    }
}
