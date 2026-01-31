using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Commands.Character.Combat;

[CharacterCommand("kill", "Combat", Priority = 1)]
[Syntax("[cmd] <character>")]
[Help(
@"[cmd] is used to kill mobiles (monsters) you see all over the world.
To kill other players, you have to use MURDER.")]
public class Kill : CharacterGameAction
{
    protected override IGuard<ICharacter>[] Guards => [new RequiresMinPosition(Positions.Standing), new RequiresAtLeastOneArgument { Message = "Kill whom?" }];

    private ICharacter Whom { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        Whom = FindHelpers.FindByName(Actor.Room.People, actionInput.Parameters[0])!;
        if (Whom == null)
            return StringHelpers.CharacterNotFound;

        if (Whom == Actor)
            return "You hit yourself. Ouch!";

        var safeResult = Whom.IsSafe(Actor);
        if (safeResult != null)
            return safeResult;

        if (Whom is IPlayableCharacter)
            return "You must MURDER a player!";

        if (Whom.Fighting != null)
        {
            // if not in same group or not same master, don't allow kill stealing
            var isInSameGroup = Actor.IsSameGroupOrPet(Whom.Fighting);
            if (!isInSameGroup)
                return "Kill stealing is not permitted.";
        }

        var nonPlayableActor = Actor as INonPlayableCharacter;
        if (Actor.CharacterFlags.IsSet("Charm") && nonPlayableActor?.Master == Whom)
            return Actor.ActPhrase("{0:N} is your beloved master.", Whom);

        if (Actor.Fighting != null)
            return "You do the best you can!";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        // GCD
        Actor.SetGlobalCooldown(1);
        //TODO: check_killer( ch, victim );

        // Starts fight
        Actor.MultiHit(Whom);
    }
}
