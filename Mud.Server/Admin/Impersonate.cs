using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Admin
{
    [AdminCommand("impersonate", "Avatar", Priority = 0)]
    [Syntax(
        "[cmd]",
        "[cmd] <character>")]
    public class Impersonate : AdminGameAction
    {
        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (Actor.Incarnating != null)
                return $"You are already incarnating {Actor.Incarnating.DisplayName}.";

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            // TODO:
            // playerCommand = new Player.Impersonate();
            // build player game action
            // playerCommand.Guards();
            // if no guards
            //      playerCommand .Execute();
            //return base.DoImpersonate(rawParameters, parameters);
        }
    }
}
