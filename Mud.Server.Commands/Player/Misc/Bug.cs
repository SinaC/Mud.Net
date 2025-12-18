using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Player.Misc;

[PlayerCommand("bug", "Misc", Priority = 50)]
[Syntax("[cmd] <message>")]
[Help(
@"This command will take your message and record it into a file as feedback
to the mud implementors.")]
public class Bug : PlayerGameAction
{
    private ICommandParser CommandParser { get; }
    private IWiznet Wiznet { get; }

    public Bug(ICommandParser commandParser, IWiznet wiznet)
    {
        CommandParser = commandParser;
        Wiznet = wiznet;
    }

    protected string Message { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        Message = CommandParser.JoinParameters(actionInput.Parameters);
        if (string.IsNullOrWhiteSpace(Message))
            return "Report which bug?";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Wiznet.Log($"****USER BUG REPORTING -- {Actor.DisplayName}: {Message}", WiznetFlags.Bugs, AdminLevels.Implementor);
        Actor.Send("Bug logged.");
    }
}
