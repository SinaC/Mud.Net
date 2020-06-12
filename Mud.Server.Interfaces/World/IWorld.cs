using System;
using System.Collections.Generic;
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

namespace Mud.Server.Interfaces.World
{
    public interface IWorld
    {
        IRoom NullRoom { get; }

        // Treasures
        IReadOnlyCollection<TreasureTable<int>> TreasureTables { get; }

        void AddTreasureTable(TreasureTable<int> table);

        // Blueprints
        IReadOnlyCollection<QuestBlueprint> QuestBlueprints { get; }
        IReadOnlyCollection<AreaBlueprint> AreaBlueprints { get; }
        IReadOnlyCollection<CharacterBlueprintBase> CharacterBlueprints { get; }

        QuestBlueprint GetQuestBlueprint(int id);
        AreaBlueprint GetAreaBlueprint(int id);
        CharacterBlueprintBase GetCharacterBlueprint(int id);
        TBlueprint GetCharacterBlueprint<TBlueprint>(int id)
            where TBlueprint : CharacterBlueprintBase;

        void AddQuestBlueprint(QuestBlueprint blueprint);
        void AddAreaBlueprint(AreaBlueprint blueprint);
        void AddCharacterBlueprint(CharacterBlueprintBase blueprint);

        //
        IEnumerable<IArea> Areas { get; }
        IEnumerable<ICharacter> Characters { get; }
        IEnumerable<INonPlayableCharacter> NonPlayableCharacters { get; }
        IEnumerable<IPlayableCharacter> PlayableCharacters { get; }

        IArea AddArea(Guid guid, AreaBlueprint blueprint);

        IExit AddExit(IRoom from, IRoom to, ExitBlueprint blueprint, ExitDirections direction);

        IPlayableCharacter AddPlayableCharacter(Guid guid, PlayableCharacterData playableCharacterData, IPlayer player, IRoom room);
        INonPlayableCharacter AddNonPlayableCharacter(Guid guid, CharacterBlueprintBase blueprint, IRoom room);
        INonPlayableCharacter AddNonPlayableCharacter(Guid guid, CharacterBlueprintBase blueprint, PetData petData, IRoom room);

        void FixWorld(); // should be called before the first ResetWorld
        void ResetWorld();

        void RemoveCharacter(ICharacter character);

        void Cleanup(); // called once outputs has been processed (before next loop)
    }
}
