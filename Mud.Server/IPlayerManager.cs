using System.Collections.Generic;
using Mud.Network;
using Mud.Server.Input;

namespace Mud.Server
{
    public interface IPlayerManager
    {
        IPlayer GetPlayer(CommandParameter parameter, bool perfectMatch);
        IEnumerable<IPlayer> Players { get; }

        // TODO: remove
        // TEST PURPOSE
        IPlayer AddPlayer(IClient client, string name);

    }
}
