using System;
using System.Collections.Generic;
using Mud.Domain;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.LootTable;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Blueprints.Room;
using Mud.Server.Item;

namespace Mud.Server
{
    public interface IWorld
    {
        IRoom NullRoom { get; }

        // Treasures
        IReadOnlyCollection<TreasureTable<int>> TreasureTables { get; }

        void AddTreasureTable(TreasureTable<int> table);

        // Blueprints
        IReadOnlyCollection<QuestBlueprint> QuestBlueprints { get; }
        IReadOnlyCollection<RoomBlueprint> RoomBlueprints { get; }
        IReadOnlyCollection<CharacterBlueprintBase> CharacterBlueprints { get; }
        IReadOnlyCollection<ItemBlueprintBase> ItemBlueprints { get; }

        QuestBlueprint GetQuestBlueprint(int id);
        RoomBlueprint GetRoomBlueprint(int id);
        CharacterBlueprintBase GetCharacterBlueprint(int id);
        ItemBlueprintBase GetItemBlueprint(int id);
        TBlueprint GetCharacterBlueprint<TBlueprint>(int id)
            where TBlueprint : CharacterBlueprintBase;
        TBlueprint GetItemBlueprint<TBlueprint>(int id)
            where TBlueprint : ItemBlueprintBase;

        void AddQuestBlueprint(QuestBlueprint blueprint);
        void AddRoomBlueprint(RoomBlueprint blueprint);
        void AddCharacterBlueprint(CharacterBlueprintBase blueprint);
        void AddItemBlueprint(ItemBlueprintBase blueprint);

        //
        IEnumerable<IArea> Areas { get; }
        IEnumerable<IRoom> Rooms { get; }
        IEnumerable<ICharacter> Characters { get; }
        IEnumerable<INonPlayableCharacter> NonPlayableCharacters { get; }
        IEnumerable<IPlayableCharacter> PlayableCharacters { get; }
        IEnumerable<IItem> Items { get; }

        IRoom GetRandomRoom(ICharacter character);
        IRoom GetDefaultRecallRoom();

        IArea AddArea(Guid guid, string displayName, int minLevel, int maxLevel, string builders, string credits);

        IRoom AddRoom(Guid guid, RoomBlueprint blueprint, IArea area);

        IExit AddExit(IRoom from, IRoom to, ExitBlueprint blueprint, ExitDirections direction);

        IPlayableCharacter AddPlayableCharacter(Guid guid, CharacterData characterData, IPlayer player, IRoom room);
        INonPlayableCharacter AddNonPlayableCharacter(Guid guid, CharacterBlueprintBase blueprint, IRoom room);

        IItemCorpse AddItemCorpse(Guid guid, ItemCorpseBlueprint blueprint, IRoom room, ICharacter victim);
        IItemCorpse AddItemCorpse(Guid guid, ItemCorpseBlueprint blueprint, IRoom room, ICharacter victim, ICharacter killer);

        IItem AddItem(Guid guid, ItemBlueprintBase blueprint, IContainer container);
        IItem AddItem(Guid guid, ItemData itemData, IContainer container);
        IItem AddItem(Guid guid, int blueprintId, IContainer container);

        IAura AddAura(IEntity target, IAbility ability, IEntity source, int level, TimeSpan duration, AuraFlags auraFlags, bool recompute, params IAffect[] affects);

        //IPeriodicAura AddPeriodicAura(IEntity target, IAbility ability, IEntity source, int amount, AmountOperators amountOperator, int level, bool tickVisible, int tickDelay, int totalTicks); // Hot
        //IPeriodicAura AddPeriodicAura(IEntity target, IAbility ability, IEntity source, SchoolTypes school, int amount, AmountOperators amountOperator, int level, bool tickVisible, int tickDelay, int totalTicks); // Dot

        void RemoveCharacter(ICharacter character);
        void RemoveItem(IItem item);
        void RemoveRoom(IRoom room);

        void Cleanup(); // called once outputs has been processed (before next loop)
    }
}
