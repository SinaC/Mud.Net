namespace Mud.Server.Race.Interfaces;

public interface IRaceManager
{
    IEnumerable<IPlayableRace> PlayableRaces { get; }
    IEnumerable<IRace> Races { get; }

    IRace? this[string name] { get; }
}
