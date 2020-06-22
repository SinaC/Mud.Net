using Mud.Common;
using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using System.Linq;
using System.Text;

namespace Mud.Server.Admin.Information
{
    [AdminCommand("wiznet", "Information")]
    [Syntax(
            "[cmd]",
            "[cmd] all",
            "[cmd] <field>")]
    public class Wiznet : AdminGameAction
    {
        public bool Display { get; protected set; }
        public WiznetFlags? FlagToToggle { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
            {
                Display = true;
                return null;
            }

            if (actionInput.Parameters[0].IsAll)
            {
                Display = false;
                return null;
            }

            WiznetFlags flag;
            if (!EnumHelpers.TryFindByName(actionInput.Parameters[0].Value.ToLowerInvariant(), out flag) || flag == WiznetFlags.None)
                return "No such option.";

            Display = false;
            FlagToToggle = flag;

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            if (Display)
            {
                StringBuilder sb = new StringBuilder();
                foreach (WiznetFlags loopFlag in EnumHelpers.GetValues<WiznetFlags>().Where(x => x != WiznetFlags.None))
                {
                    bool isOnLoop = Actor.WiznetFlags.HasFlag(loopFlag);
                    sb.AppendLine($"{loopFlag,-16} : {(isOnLoop ? "ON" : "OFF")}");
                }
                Actor.Send(sb);
                return;
            }
            if (!FlagToToggle.HasValue)
            {
                foreach (WiznetFlags wiznetFlag in EnumHelpers.GetValues<WiznetFlags>().Where(x => x != WiznetFlags.None))
                    Actor.AddWiznet(wiznetFlag);
                Actor.Send("You will now see every wiznet informations.");
                return;
            }

            bool isOn = (Actor.WiznetFlags & FlagToToggle.Value) == FlagToToggle.Value;
            if (isOn)
            {
                Actor.Send($"You'll no longer see {FlagToToggle.Value} on wiznet.");
                Actor.RemoveWiznet(FlagToToggle.Value);
            }
            else
            {
                Actor.Send($"You will now see {FlagToToggle.Value} on wiznet.");
                Actor.AddWiznet(FlagToToggle.Value);
            }
        }
    }
}
