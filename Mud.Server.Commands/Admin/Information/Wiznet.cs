using Mud.Common;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using System.Text;

namespace Mud.Server.Commands.Admin.Information;

[AdminCommand("wiznet", "Information")]
[Syntax(
        "[cmd]",
        "[cmd] all",
        "[cmd] <field>")]
[Help(
@"Wiznet is sort of an immortal news service, to show important events to
the wiznetted immoral.  Wiznet by itself turns wiznet on and off, 
wiznet show lists all settable flags (they are not detailed here), 
wiznet status shows your current wiznet settings, and wiznet <field> toggles
a field on and off.  The events should be self-explanatory, if they are not,
fiddle with them a while.  More events are available at higher levels.")]
public class Wiznet : AdminGameAction
{
    protected bool Display { get; set; }
    protected WiznetFlags? FlagToToggle { get; set; }

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
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

        if (!EnumHelpers.TryFindByName(actionInput.Parameters[0].Value.ToLowerInvariant(), out WiznetFlags flag) || flag == WiznetFlags.None)
            return "No such option.";

        Display = false;
        FlagToToggle = flag;

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        if (Display)
        {
            StringBuilder sb = new();
            foreach (WiznetFlags loopFlag in EnumHelpers.GetValues<WiznetFlags>().Where(x => x != WiznetFlags.None))
            {
                var isOnLoop = Actor.WiznetFlags.HasFlag(loopFlag);
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

        var isOn = (Actor.WiznetFlags & FlagToToggle.Value) == FlagToToggle.Value;
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
