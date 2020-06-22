using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Player.Information
{
    [PlayerCommand("areas", "Information", Priority = 10)]
    public class Areas : PlayerGameAction
    {
        private IAreaManager AreaManager { get; }

        public Areas(IAreaManager areaManager)
        {
            AreaManager = areaManager;
        }

        public override void Execute(IActionInput actionInput)
        {
            var sb = TableGenerators.AreaTableGenerator.Value.Generate("Areas", AreaManager.Areas);
            Actor.Page(sb);
        }
    }
}
