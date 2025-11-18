using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Communication;

[CharacterCommand("say", "Communication", MinPosition = Positions.Resting)]
[Syntax("[cmd] <message>")]
public class Say : CharacterGameAction
{
    protected string What { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return "Say what?";

        What = CommandHelpers.JoinParameters(actionInput.Parameters);

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Actor.Act(ActOptions.ToAll, "%g%{0:N} say{0:v} '%x%{1}%g%'%x%", Actor, What);
    }
}
