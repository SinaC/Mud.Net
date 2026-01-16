using Mud.Domain;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Guards.AdminGuards;

public class MinAdminLevelGuard(AdminLevels minAdminLevel) : IAdminGuard
{
    public AdminLevels MinAdminLevel { get; } = minAdminLevel;

    public string? Guards(IAdmin actor)
    {
        if (actor.Level < MinAdminLevel)
            return "Command not found";
        return null;
    }
}