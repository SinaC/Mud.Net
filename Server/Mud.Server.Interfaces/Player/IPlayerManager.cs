using Mud.Server.CommandParser.Interfaces;

namespace Mud.Server.Interfaces.Player;

public interface IPlayerManager
{
    IPlayer? GetPlayer(ICommandParameter parameter, bool perfectMatch);
    IEnumerable<IPlayer> Players { get; }
}
