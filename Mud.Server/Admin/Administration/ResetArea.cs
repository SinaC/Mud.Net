using Mud.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.GameAction;
using System.Linq;

namespace Mud.Server.Admin.Administration
{
    [AdminCommand("resetarea", "Admin")]
    [Syntax(
            "[cmd] <area>",
            "[cmd] (if impersonated)")]
    public class ResetArea : AdminGameAction
    {
        private IAreaManager AreaManager { get; }

        public IArea Area { get; protected set; }

        public ResetArea(IAreaManager areaManager)
        {
            AreaManager = areaManager;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0 && Actor.Impersonating == null)
                return BuildCommandSyntax();

            if (actionInput.Parameters.Length == 0)
                Area = Impersonating.Room.Area;
            else
                Area = AreaManager.Areas.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.DisplayName, actionInput.Parameters[0].Value));

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
}
