using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Player.Misc
{
    [PlayerCommand("typo", "Misc", Priority = 50)]
    [Syntax("[cmd] <message>")]
    public class Typo : PlayerGameAction
    {
        private IWiznet Wiznet { get; }

        public string Message { get; protected set; }

        public Typo(IWiznet wiznet)
        {
            Wiznet = wiznet;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (string.IsNullOrWhiteSpace(actionInput.RawParameters))
                return "Report which typo?";

            Message = actionInput.RawParameters;
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            Wiznet.Wiznet($"****USER TYPO REPORTING -- {Actor.DisplayName}: {Message}", WiznetFlags.Typos, AdminLevels.Implementor);
            Actor.Send("Typo logged.");
        }
    }
}
