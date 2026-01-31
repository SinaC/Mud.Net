using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.TableGenerator;

namespace Mud.Server.Commands.Admin.Information;

[AdminCommand("areas", "Information", Priority = 10)]
[Help(@"[cmd] shows you a list of areas in the game.")]
public class Areas : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [];

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
