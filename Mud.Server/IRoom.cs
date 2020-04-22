using System.Collections.Generic;
using Mud.Domain;
using Mud.Server.Blueprints.Room;

namespace Mud.Server
{
    public interface IRoom : IEntity, IContainer
    {
        RoomBlueprint Blueprint { get; }

        IReadOnlyDictionary<string, string> ExtraDescriptions { get; } // keyword -> description

        IArea Area { get; }

        IEnumerable<ICharacter> People { get; }
        IEnumerable<INonPlayableCharacter> NonPlayableCharacters { get; }
        IEnumerable<IPlayableCharacter> PlayableCharacters { get; }
        IExit[] Exits { get; } // fixed length

        IExit Exit(ExitDirections direction);
        IRoom GetRoom(ExitDirections direction);

        bool Enter(ICharacter character);
        bool Leave(ICharacter character);
    }
}
