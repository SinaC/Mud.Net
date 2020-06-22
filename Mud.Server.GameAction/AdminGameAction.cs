using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction
{
    public abstract class AdminGameAction : PlayerGameActionBase<IAdmin, IAdminGameActionInfo>
    {
        public override string Guards(IActionInput actionInput)
        {
            // Must be executed before any other tests because we don't want to 'give' any advice to admin that this command exists
            if ((actionInput.Actor as IAdmin)?.Level < (actionInput.GameActionInfo as IAdminGameActionInfo)?.MinLevel)
                return "Command not found";

            string baseGuards = base.Guards(actionInput);
            return baseGuards;
        }
    }
}
