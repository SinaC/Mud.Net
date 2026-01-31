using Mud.Common;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Commands.Admin.Administration;

[AdminCommand("resetarea", "Admin", Priority = 800)]
[Syntax(
        "[cmd] <area>",
        "[cmd] (if impersonated)")]
[Help(
@"This command resets an area, making died mob repoping. If hard is specified,
each room is purged of its mobiles/objects before resetting.")]
// TODO: hard parameter
public class ResetArea : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [];

    private IAreaManager AreaManager { get; }
    private IResetManager ResetManager { get; }

    public ResetArea(IAreaManager areaManager, IResetManager resetManager)
    {
        AreaManager = areaManager;
        ResetManager = resetManager;
    }

    private IArea Area { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0 && Actor.Impersonating == null)
            return BuildCommandSyntax();

        if (actionInput.Parameters.Length == 0)
        {
            if (Impersonating != null)
                Area = Impersonating.Room.Area;
            else
                return "You are not impersonating, please specity an area name.";
        }
        else
            Area = AreaManager.Areas.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.DisplayName, actionInput.Parameters[0].Value))!;

        if (Area == null)
            return "Area not found.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        ResetManager.ResetArea(Area);

        Actor.Send($"{Area.DisplayName} resetted.");
    }
}
