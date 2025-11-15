using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Admin.Information;

[AdminCommand("areas", "Information", Priority = 10)]
public class Areas : AdminGameAction
{
    private IAreaManager AreaManager { get; }

    public Areas(IAreaManager areaManager)
    {
        AreaManager = areaManager;
    }

    public override void Execute(IActionInput actionInput)
    {
        var sb = TableGenerators.FullInfoAreaTableGenerator.Value.Generate("Areas", AreaManager.Areas);
        Actor.Page(sb);
    }
}
