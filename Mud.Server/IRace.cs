using System.Collections.Generic;
using Mud.Server.Abilities;

namespace Mud.Server
{
    public interface IRace
    {
        string Name { get; }
        string DisplayName { get; }
        string ShortName { get; }

        IEnumerable<AbilityAndLevel> Abilities { get; }

        // TODO: specific behaviour such as 120% xp for human, infrared for dwarf, ...
        // TODO: xp/level, ...
    }
}
