using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;
using System.Text;

namespace Mud.Server.Commands.Player.Communication;

[PlayerCommand("replay", "Communication")]
[Help(
@"REPLAY is used to read the tells you have received when being
AFK/BUILDING/LINKDEAD.")]
public class Replay : PlayerGameAction
{
    protected override IGuard<IPlayer>[] Guards => [];

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (!Actor.DelayedTells.Any())
            return "You have no tells to replay.";
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        StringBuilder sb = new StringBuilder();
        foreach (string sentence in Actor.DelayedTells)
            sb.AppendLine(sentence);
        Actor.Page(sb);
        Actor.ClearDelayedTells();
    }
}
