using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using System.Text;

namespace Mud.Server.Commands.Player.Communication;

[PlayerCommand("replay", "Communication")]
[Help(
@"REPLAY is used to read the tells you have received when being
AFK/BUILDING/LINKDEAD.")]
public class Replay : PlayerGameAction
{
    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
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
