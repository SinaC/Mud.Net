using Mud.POC.Abilities2.Domain;
using System.Collections.Generic;

namespace Mud.POC.Abilities2.Interfaces
{
    public interface IRoom : IEntity, IContainer
    {
        IArea Area { get; }

        IEnumerable<ICharacter> People { get; }

        RoomFlags RoomFlags { get; }
    }
}
