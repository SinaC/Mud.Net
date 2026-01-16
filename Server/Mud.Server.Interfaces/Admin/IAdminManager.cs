using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Interfaces.Admin;

public interface IAdminManager
{
    void AddAdmin(IAdmin admin);
    void RemoveAdmin(IAdmin admin);
    IAdmin? GetAdmin(ICommandParameter parameter, bool perfectMatch);
    IEnumerable<IAdmin> Admins { get; }
}
