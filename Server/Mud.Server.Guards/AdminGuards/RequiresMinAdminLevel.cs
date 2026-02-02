using Mud.Domain;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Guards.AdminGuards;

public class RequiresMinAdminLevel(AdminLevels minAdminLevel) : IGuard<IAdmin>
{
    public AdminLevels MinAdminLevel { get; } = minAdminLevel;

    public string? Guards(IAdmin admin, IActionInput actionInput, IGameAction gameAction)
    {
        if (admin.Level < MinAdminLevel)
            return "Command not found";
        return null;
    }
}