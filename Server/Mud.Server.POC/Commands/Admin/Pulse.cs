using Mud.Common;
using Mud.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.AdminGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.POC.Commands.Admin;

[AdminCommand("Pulse", "Admin")]
[Syntax(
    "[cmd]",
    "[cmd] <pulse>")]
public class Pulse : AdminGameAction
{
    private IPulseManager PulseManager { get; }

    protected override IGuard<IAdmin>[] Guards => [new RequiresMinAdminLevel(AdminLevels.Implementor)];

    public Pulse(IPulseManager pulseManager)
    {
        PulseManager = pulseManager;
    }

    private bool DisplayAll { get; set; }
    private string PulseName { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
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
