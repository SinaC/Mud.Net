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

        RoomFlags RoomFlags { get; }
    }
}
