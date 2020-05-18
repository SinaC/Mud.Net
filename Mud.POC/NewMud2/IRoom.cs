using System.Collections.Generic;

namespace Mud.POC.NewMud2
{
    public interface IRoom : IEntity
    {
        IArea Area { get; }
        IEnumerable<ICharacter> People { get; }
        IEnumerable<IItem> Content { get; }
    }
}
