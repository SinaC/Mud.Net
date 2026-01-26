using Mud.Common;
using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Flags;
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
    private IFlagsManager FlagsManager { get; }

    public Wiznet(IFlagsManager flagsManager)
    {
        FlagsManager = flagsManager;
    }

    protected bool Display { get; set; }
    protected string? FlagToToggle { get; set; }

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

        var flag = FlagsManager.AvailableValues<IWiznetFlags>().FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x, actionInput.Parameters[0].Value));
        if (flag != null)
        {
            Display = false;
            FlagToToggle = flag;

            return null;
        }
        return "No such option.";

    }

    public override void Execute(IActionInput actionInput)
    {
        if (Display)
        {
            var sb = new StringBuilder();
            foreach (var loopFlag in FlagsManager.AvailableValues<IWiznetFlags>().OrderBy(x => x))
            {
                var isOnLoop = Actor.WiznetFlags.IsSet(loopFlag);
                sb.AppendLine($"{loopFlag,-16} : {(isOnLoop ? "%g%ON%x%" : "%r%OFF%x%")}");
            }
            Actor.Send(sb);
            return;
        }
        if (FlagToToggle is null) // all
        {
            foreach (var wiznetFlag in FlagsManager.AvailableValues<IWiznetFlags>().OrderBy(x => x))
                Actor.AddWiznet(new WiznetFlags(wiznetFlag));
            Actor.Send("You will now see every wiznet informations.");
            return;
        }

        var isOn = Actor.WiznetFlags.IsSet(FlagToToggle);
        if (isOn)
        {
            Actor.Send($"You'll no longer see {FlagToToggle} on wiznet.");
            Actor.RemoveWiznet(new WiznetFlags(FlagToToggle));
        }
        else
        {
            Actor.Send($"You will now see {FlagToToggle} on wiznet.");
            Actor.AddWiznet(new WiznetFlags(FlagToToggle));
        }
    }
}
