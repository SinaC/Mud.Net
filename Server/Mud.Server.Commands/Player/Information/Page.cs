using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Commands.Player.Information;

[PlayerCommand("page", "Information")]
[Alias("scroll")]
[Syntax(
        "[cmd]",
        "[cmd] <number>")]
[Help(
@"This command changes the number of lines the mud sends you in a page (the 
default is 24 lines).  Change this to a higher number for larger screen
sizes, or to 0 to disabling paging.")]
public class Page : PlayerGameAction
{
    protected override IGuard<IPlayer>[] Guards => [];

    private bool Display { get; set; }
    private int LineCount { get; set; }

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
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
