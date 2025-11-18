using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Player.Misc;

[PlayerCommand("typo", "Misc", Priority = 50)]
[Syntax("[cmd] <message>")]
public class Typo : PlayerGameAction
{
    private IWiznet Wiznet { get; }

    public Typo(IWiznet wiznet)
    {
        Wiznet = wiznet;
    }

    protected string Message { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        Message = CommandHelpers.JoinParameters(actionInput.Parameters);
        if (string.IsNullOrWhiteSpace(Message))
            return "Report which typo?";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Wiznet.Log($"****USER TYPO REPORTING -- {Actor.DisplayName}: {Message}", WiznetFlags.Typos, AdminLevels.Implementor);
        Actor.Send("Typo logged.");
    }
}
