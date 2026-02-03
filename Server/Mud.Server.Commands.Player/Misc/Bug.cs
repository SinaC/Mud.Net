using Mud.Domain;
using Mud.Flags;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.PlayerGuards;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;
using Mud.Server.Parser.Interfaces;

namespace Mud.Server.Commands.Player.Misc;

[PlayerCommand("bug", "Misc", Priority = 50)]
[Syntax("[cmd] <message>")]
[Help(
@"This command will take your message and record it into a file as feedback
to the mud implementors.")]
public class Bug : PlayerGameAction
{
    protected override IGuard<IPlayer>[] Guards => [new RequiresAtLeastOneArgument { Message = "Report which bug ?" }];

    private IParser Parser { get; }
    private IWiznet Wiznet { get; }

    public Bug(IParser parser, IWiznet wiznet)
    {
        Parser = parser;
        Wiznet = wiznet;
    }

    private string Message { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        Message = Parser.JoinParameters(actionInput.Parameters);

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Wiznet.Log($"****USER BUG REPORTING -- {Actor.DisplayName}: {Message}", new WiznetFlags("Bugs"), AdminLevels.Implementor);
        Actor.Send("Bug logged.");
    }
}
