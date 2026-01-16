using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Interfaces.Player;

public interface IPlayerManager
{
    IPlayer? GetPlayer(ICommandParameter parameter, bool perfectMatch);
    IEnumerable<IPlayer> Players { get; }
}
