using System.Collections.Generic;
using Mud.Network;
using Mud.Server.Input;

namespace Mud.Server.Interfaces.Admin
{
    public interface IAdminManager
    {
        IAdmin GetAdmin(CommandParameter parameter, bool perfectMatch);
        IEnumerable<IAdmin> Admins { get; }

        // TODO: remove
        // TEST PURPOSE
        IAdmin AddAdmin(IClient client, string name);
    }
}
