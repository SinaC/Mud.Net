using System.Collections.Generic;

namespace Mud.POC.NewMud2
{
    public interface IWorld
    {
        IEnumerable<IRoom> Rooms { get; }
        IEnumerable<ICharacter> Characters { get; }
        IEnumerable<IItem> Items { get; }
    }
}
