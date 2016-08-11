using System.Collections.Generic;

namespace Mud.Server
{
    public interface IRaceManager
    {
        IEnumerable<IRace> Races { get; }

        IRace this[string name] { get; }
    }
}
