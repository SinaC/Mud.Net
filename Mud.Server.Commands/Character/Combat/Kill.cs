using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Combat;

[CharacterCommand("kill", "Combat", Priority = 1, MinPosition = Positions.Standing)]
[Syntax("[cmd] <character>")]
[Help(
@"[cmd] is used to kill mobiles (monsters) you see all over the world.
To kill other players, you have to use MURDER.")]
public class Kill : CharacterGameAction
{
    protected ICharacter Whom { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return "Kill whom?";

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

        var playableActor = Actor as IPlayableCharacter;
        if (Whom.Fighting != null)
        {
            // if not in same group, don't allow kill stealing
            bool isInSameGroup = playableActor != null && Whom.Fighting is IPlayableCharacter fightingPlayableCharacter && playableActor.IsSameGroup(fightingPlayableCharacter);
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
