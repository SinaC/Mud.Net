using Mud.POC.Abilities2.Domain;
using System.Collections.Generic;

namespace Mud.POC.Abilities2.Interfaces
{
    public interface IRoom
    {
        IArea Area { get; }

        IEnumerable<ICharacter> People { get; }
        IEnumerable<IItem> Content { get; }

        RoomFlags RoomFlags { get; }
    }
}
