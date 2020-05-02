using System.Collections.Generic;

namespace Mud.POC.Abilities
{
    public interface IEntity
    {
        string Name { get; }
        IEnumerable<string> Keywords { get; } // name tokenize
    }
}
