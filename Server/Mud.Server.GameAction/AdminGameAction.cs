using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction;

public abstract class AdminGameAction : PlayerGameActionBase<IAdmin, IAdminGameActionInfo>
{
    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        return baseGuards;
    }
}
