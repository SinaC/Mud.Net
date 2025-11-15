using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Combat;

[CharacterCommand("consider", "Information", "Combat", MinPosition = Positions.Resting)]
[Syntax("[cmd] <character>")]
public class Consider : CharacterGameAction
{
    protected ICharacter Whom { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return "Consider killing whom?";

        Whom = FindHelpers.FindByName(Actor.Room.People, actionInput.Parameters[0])!;
        if (Whom == null)
            return StringHelpers.CharacterNotFound;

        if (Whom.IsSafe(Actor) != null)
            return "Don't even think about it.";

        if (Whom == Actor)
            return "You are such a badass.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        int diff = Whom.Level - Actor.Level;
        if (diff <= -10)
            Actor.Act(ActOptions.ToCharacter, "You can kill {0} naked and weaponless.", Whom);
        else if (diff <= -5)
            Actor.Act(ActOptions.ToCharacter, "{0:N} is no match for you.", Whom);
        else if (diff <= -2)
            Actor.Act(ActOptions.ToCharacter, "{0:N} looks like an easy kill.", Whom);
        else if (diff <= 1)
            Actor.Send("The perfect match!");
        else if (diff <= 4)
            Actor.Act(ActOptions.ToCharacter, "{0:N} says 'Do you fell lucky punk?'.", Whom);
        else if (diff <= 9)
            Actor.Act(ActOptions.ToCharacter, "{0:N} laughs at you mercilessly.", Whom);
        else
            Actor.Send("Death will thank you for your gift.");
    }
}
