using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Player;
using Mud.Server.TableGenerator;

namespace Mud.Server.Commands.Player.Information;

[PlayerCommand("areas", "Information", Priority = 10)]
[Help(@"[cmd] shows you a list of areas in the game.")]
public class Areas : PlayerGameAction
{
    protected override IGuard<IPlayer>[] Guards => [];

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
