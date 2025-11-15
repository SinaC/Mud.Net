using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Communication;

[CharacterCommand("emote", "Communication", MinPosition = Positions.Resting)]
[Syntax("[cmd] <message>")]
public class Emote : CharacterGameAction
{
    protected string What { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return "Emote what?";

        What = CommandHelpers.JoinParameters(actionInput.Parameters);

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Actor.Act(ActOptions.ToAll, "{0:n} {1}", Actor, What);
    }
}
