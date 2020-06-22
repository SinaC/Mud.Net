using Mud.Repository;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Player.Account
{
    [PlayerCommand("password", "Misc", Priority = 999, NoShortcut = true)]
    [Syntax("[cmd] <old-password> <new-password>")]
    public class Password : AccountGameActionBase
    {
        private ILoginRepository LoginRepository { get; }

        public string NewPassword { get; protected set; }

        public Password(ILoginRepository loginRepository)
        {
            LoginRepository = loginRepository;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length != 2)
                return BuildCommandSyntax();
            NewPassword = actionInput.Parameters[1].Value;
            if (NewPassword.Length < 5)
                return "New password must be at least five characters long.";
            if (!LoginRepository.CheckPassword(Actor.Name, actionInput.Parameters[0].Value))
            {
                Actor.SetGlobalCooldown(10 * Pulse.PulsePerSeconds);
                return "Wrong password. Wait 10 seconds.";
            }
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            LoginRepository.ChangePassword(Actor.Name, NewPassword);
            NewPassword = null;
        }
    }
}
