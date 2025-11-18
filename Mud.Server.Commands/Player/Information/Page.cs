using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Player.Information;

[PlayerCommand("page", "Information")]
[Alias("scroll")]
[Syntax(
        "[cmd]",
        "[cmd] <number>")]
public class Page : PlayerGameAction
{
    protected bool Display { get; set; }
    protected int LineCount { get; set; }

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
        {
            Display = true;
            return null;
        }

        if (!actionInput.Parameters[0].IsNumber)
            return "You must provide a number.";

        LineCount = actionInput.Parameters[0].AsNumber;
        if (LineCount == 0)
            return null;

        if (LineCount < 10 || LineCount > 100)
            return "Please provide a reasonable number.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        if (Display)
        {
            if (Actor.PagingLineCount == 0)
                Actor.Send("You do not page long messages.");
            else
                Actor.Send($"You currently display {Actor.PagingLineCount} lines per page.");
            return;
        }

        if (LineCount == 0)
        {
            Actor.SetPagingLineCount(0);
            Actor.Send("Paging disabled");
            return;
        }

        Actor.Send($"Scroll set to {LineCount} lines.");
        Actor.SetPagingLineCount(LineCount);
    }
}
