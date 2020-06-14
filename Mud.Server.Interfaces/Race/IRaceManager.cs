using System.Collections.Generic;

namespace Mud.Server.Interfaces.Race
{
    public interface IRaceManager
    {
        IEnumerable<IPlayableRace> PlayableRaces { get; }
        IEnumerable<IRace> Races { get; }

        IRace this[string name] { get; }
    }
}
