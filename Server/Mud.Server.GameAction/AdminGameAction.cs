using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction;

public abstract class AdminGameAction : PlayerGameActionBase<IAdmin, IAdminGameActionInfo>
{
    public override string? Guards(IActionInput actionInput)
    {
        // Must be executed before any other tests because we don't want to 'give' any advice to admin that this command exists
        var adminGuards = (actionInput.GameActionInfo as AdminGameActionInfo)?.AdminGuards;
        if (actionInput.Actor is IAdmin admin && adminGuards != null && adminGuards.Length > 0)
        {
            foreach (var guard in adminGuards)
            {
                var guardResult = guard.Guards(admin, actionInput, this);
                if (guardResult != null)
                    return guardResult;
            }
        }

        var baseGuards = base.Guards(actionInput);
        return baseGuards;
    }
}
