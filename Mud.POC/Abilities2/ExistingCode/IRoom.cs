using Mud.POC.Abilities2.Domain;
using Mud.Server.Blueprints.Room;
using System.Collections.Generic;

namespace Mud.POC.Abilities2.ExistingCode
{
    public interface IRoom : IEntity, IContainer
    {
        RoomBlueprint Blueprint { get; }

        IArea Area { get; }

        IEnumerable<ICharacter> People { get; }
        IEnumerable<INonPlayableCharacter> NonPlayableCharacters { get; }

        RoomFlags RoomFlags { get; }
        SectorTypes SectorType { get; set; }

        IExit this[ExitDirections direction] { get; }
    }
}
