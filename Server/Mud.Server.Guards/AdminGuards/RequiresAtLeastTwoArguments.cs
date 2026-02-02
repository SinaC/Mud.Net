using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Guards.AdminGuards;

public class RequiresAtLeastTwoArguments : PlayerGuards.RequiresAtLeastTwoArguments, IGuard<IAdmin>
{
    public string? Guards(IAdmin admin, IActionInput actionInput, IGameAction gameAction)
        => base.Guards(admin, actionInput, gameAction);
}
