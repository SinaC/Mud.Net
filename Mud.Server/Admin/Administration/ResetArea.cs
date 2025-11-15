using Mud.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Admin.Administration;

[AdminCommand("resetarea", "Admin")]
[Syntax(
        "[cmd] <area>",
        "[cmd] (if impersonated)")]
public class ResetArea : AdminGameAction
{
    private IAreaManager AreaManager { get; }

    public ResetArea(IAreaManager areaManager)
    {
        AreaManager = areaManager;
    }

    protected IArea Area { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0 && Actor.Impersonating == null)
            return BuildCommandSyntax();

        if (actionInput.Parameters.Length == 0)
            Area = Impersonating.Room.Area;
        else
            Area = AreaManager.Areas.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.DisplayName, actionInput.Parameters[0].Value))!;

        if (Area == null)
            return "Area not found.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Area.ResetArea();

        Actor.Send($"{Area.DisplayName} resetted.");
    }
}
