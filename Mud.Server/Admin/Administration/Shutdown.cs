using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Admin.Administration
{
    [AdminCommand("shutdown", "Admin", Priority = 999 /*low priority*/, NoShortcut = true, MinLevel = AdminLevels.Implementor, CannotBeImpersonated = true)]
    [Syntax("[cmd] <delay>")]
    public class Shutdown : AdminGameAction
    {
        private IServerAdminCommand ServerAdminCommand { get; }

        public int Seconds { get; protected set; }

        public Shutdown(IServerAdminCommand serverAdminCommand)
        {
            ServerAdminCommand = serverAdminCommand;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            int seconds;
            if (actionInput.Parameters.Length == 0 || !int.TryParse(actionInput.Parameters[0].Value, out seconds))
                return BuildCommandSyntax();
            if (seconds < 30)
                return "You cannot shutdown that fast.";

            Seconds = seconds;
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            ServerAdminCommand.Shutdown(Seconds);
        }
    }
}
