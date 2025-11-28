using Mud.Common.Attributes;
using Mud.Server.Interfaces.Race;

namespace Mud.Server.Race;

[Export(typeof(IRaceManager)), Shared]
public class RaceManager : IRaceManager
{
    private readonly Dictionary<string, IRace> _raceByNames;

    public RaceManager(IEnumerable<IRace> races)
    {
        _raceByNames = new Dictionary<string, IRace>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var r in races)
            _raceByNames.Add(r.Name, r);
    }

    #region IRaceManager

    public IEnumerable<IPlayableRace> PlayableRaces
        => Races.OfType<IPlayableRace>();

    public IEnumerable<IRace> Races
        => _raceByNames.Values;

    public IRace? this[string name]
    {
        get
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;
            if (_raceByNames.TryGetValue(name, out var r))
                return r;
            return null;
        }
    }

    #endregion
}
