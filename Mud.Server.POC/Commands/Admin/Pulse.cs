using Mud.Common;
using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.POC.Commands.Admin
{
    [AdminCommand("Pulse", "Admin", MinLevel = AdminLevels.Implementor)]
    [Syntax(
        "[cmd]",
        "[cmd] <pulse>")]
    public class Pulse : AdminGameAction
    {
        private IPulseManager PulseManager { get; }

        public Pulse(IPulseManager pulseManager)
        {
            PulseManager = pulseManager;
        }

        protected bool DisplayAll { get; set; }
        protected string PulseName { get; set; } = default!;

        public override string? Guards(IActionInput actionInput)
        {
            var baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
            {
                DisplayAll = true;
                return null;
            }

            var pulseNames = PulseManager.PulseNames.OrderBy(x => x).ToArray();
            var pulseName = pulseNames.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x, actionInput.Parameters[0].Value));
            if (pulseName == null)
                return "Invalid pulse";

            PulseName = pulseName;
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            if (DisplayAll)
            {
                Actor.Send("Pulses:");
                foreach (var name in PulseManager.PulseNames.OrderBy(x => x))
                    Actor.Send("  {0}", name);
                return;
            }
            Actor.Send("Triggering {0} pulse", PulseName);
            PulseManager.Pulse(PulseName);
        }
    }
}
