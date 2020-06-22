using System.Collections.Generic;
using Mud.Network;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Interfaces.Player
{
    public interface IPlayerManager
    {
        IPlayer GetPlayer(ICommandParameter parameter, bool perfectMatch);
        IEnumerable<IPlayer> Players { get; }

        // TODO: remove
        // TEST PURPOSE
        IPlayer AddPlayer(IClient client, string name);

    }
}
