using Mud.Domain;
using Mud.Flags;
using Mud.Server.CommandParser.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.PlayerGuards;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Commands.Player.Misc;

[PlayerCommand("typo", "Misc", Priority = 50)]
[Syntax("[cmd] <message>")]
[Help(
@"This command will take your message and record it into a file as feedback
to the mud implementors.")]
public class Typo : PlayerGameAction
{
    protected override IGuard<IPlayer>[] Guards => [new RequiresAtLeastOneArgument { Message = "Report which typo ?" }];

    private ICommandParser CommandParser { get; }
    private IWiznet Wiznet { get; }

    public Typo(ICommandParser commandParser, IWiznet wiznet)
    {
        CommandParser = commandParser;
        Wiznet = wiznet;
    }

    private string Message { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        Message = CommandParser.JoinParameters(actionInput.Parameters);

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Wiznet.Log($"****USER TYPO REPORTING -- {Actor.DisplayName}: {Message}", new WiznetFlags("Typos"), AdminLevels.Implementor);
        Actor.Send("Typo logged.");
    }
}
