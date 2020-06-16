using System.Collections.Generic;
using Mud.Network;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Interfaces.Admin
{
    public interface IAdminManager
    {
        IAdmin GetAdmin(ICommandParameter parameter, bool perfectMatch);
        IEnumerable<IAdmin> Admins { get; }

        // TODO: remove
        // TEST PURPOSE
        IAdmin AddAdmin(IClient client, string name);
    }
}
