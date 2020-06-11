using System.Collections.Generic;
using System.Linq;
using Mud.Domain;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Room;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;

namespace Mud.Server.Interfaces.Room
{
    public interface IRoom : IEntity, IContainer
    {
        RoomBlueprint Blueprint { get; }

        ILookup<string, string> ExtraDescriptions { get; } // keyword -> descriptions

        RoomFlags BaseRoomFlags { get; }
        RoomFlags RoomFlags { get; }

        IArea Area { get; }

        IEnumerable<ICharacter> People { get; }
        IEnumerable<INonPlayableCharacter> NonPlayableCharacters { get; }
        IEnumerable<IPlayableCharacter> PlayableCharacters { get; }
        IEnumerable<(INonPlayableCharacter character, TBlueprint blueprint)> GetNonPlayableCharacters<TBlueprint>()
            where TBlueprint : CharacterBlueprintBase;

        Sizes? MaxSize { get; }
        int HealRate { get; }
        int ResourceRate { get; }
        int Light { get; }
        SectorTypes SectorType { get; }
        bool IsPrivate { get; }
        bool IsDark { get; }

        IExit[] Exits { get; } // fixed length

        IExit this[ExitDirections direction] { get; }
        IRoom GetRoom(ExitDirections direction);

        bool Enter(ICharacter character);
        bool Leave(ICharacter character);

        void IncreaseLight();
        void DecreaseLight();

        void ResetRoom();

        // Affects
        void ApplyAffect(IRoomFlagsAffect affect);
    }
}
