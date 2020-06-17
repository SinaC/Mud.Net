using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Player.Account
{
    public abstract class AccountGameActionBase : PlayerGameAction
    {
        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (Impersonating != null)
            {
                if (Impersonating.Fighting != null)
                    return "No way! You are fighting.";
                if (Impersonating.Position == Positions.Stunned)
                    return "You can't leave while stunned.";
            }

            return null;
        }
    }
}
