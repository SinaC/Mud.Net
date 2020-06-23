using System.Collections.Generic;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Interfaces.Admin
{
    public interface IAdminManager
    {
        IAdmin GetAdmin(ICommandParameter parameter, bool perfectMatch);
        IEnumerable<IAdmin> Admins { get; }
    }
}
