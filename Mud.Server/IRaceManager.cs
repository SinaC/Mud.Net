using System.Collections.Generic;

namespace Mud.Server
{
    public interface IRaceManager
    {
        IReadOnlyCollection<IRace> Races { get; }

        IRace this[string name] { get; }
    }
}
