using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.TableGenerator;

namespace Mud.Server.Commands.Admin.Information;

[AdminCommand("dumpcommands")]
public class DumpCommands : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [];

    private IGameActionManager GameActionManager { get; }

    public DumpCommands(IGameActionManager gameActionManager)
    {
        GameActionManager = gameActionManager;
    }

    public override void Execute(IActionInput actionInput)
    {
        var sb = TableGenerators.GameActionInfoTableGenerator.Value.Generate("Commands", GameActionManager.GameActions.OrderBy(x => x.Name));
        Actor.Page(sb);
    }
}
