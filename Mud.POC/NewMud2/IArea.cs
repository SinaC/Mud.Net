using System.Collections.Generic;

namespace Mud.POC.NewMud2
{
    public interface IArea
    {
        IEnumerable<IRoom> Rooms { get; }
        IEnumerable<ICharacter> Characters { get; }
    }
}
