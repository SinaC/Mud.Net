using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Guards.AdminGuards;

public class RequiresAtLeastOneArgument : PlayerGuards.RequiresAtLeastOneArgument, IGuard<IAdmin>
{
    public string? Guards(IAdmin admin, IActionInput actionInput, IGameAction gameAction)
        => base.Guards(admin, actionInput, gameAction);
}
