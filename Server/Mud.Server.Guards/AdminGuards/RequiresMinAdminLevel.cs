using Mud.Domain;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

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