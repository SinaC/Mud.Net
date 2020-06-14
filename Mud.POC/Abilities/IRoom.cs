using System.Collections.Generic;

namespace Mud.POC.Abilities
{
    public interface IRoom : IEntity
    {
        IEnumerable<ICharacter> People { get; }
        IEnumerable<IItem> Content { get; }
    }
}
