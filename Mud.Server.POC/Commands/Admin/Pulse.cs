using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.POC.Commands.Admin
{
    [AdminCommand("Pulse", "Admin")]
    public class Pulse : AdminGameAction
    {
        private IPulseManager PulseManager { get; }

        public Pulse(IPulseManager pulseManager)
        {
            PulseManager = pulseManager;
        }
        protected string PulseName { get; set; } = default!;

        public override string? Guards(IActionInput actionInput)
        {
            var baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return BuildCommandSyntax();

            PulseName = actionInput.Parameters[0].Value;

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            Actor.Send("Pulse {0}", PulseName);
            PulseManager.Pulse(PulseName);
        }
    }
}
