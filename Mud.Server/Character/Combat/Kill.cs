using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Combat
{
    [CharacterCommand("kill", "Combat", Priority = 1, MinPosition = Positions.Fighting)]
    [Syntax("[cmd] <character>")]
    public class Kill : CharacterGameAction
    {
        public ICharacter Whom { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return "Kill whom?";

            Whom = FindHelpers.FindByName(Actor.Room.People, actionInput.Parameters[0]);
            if (Whom == null)
                return StringHelpers.CharacterNotFound;

            if (Whom == Actor)
                return "You hit yourself. Ouch!";

            if (Whom.IsSafe(Actor))
                return "Not on that victim.";

            if (Whom is IPlayableCharacter)
                return "You must MURDER a player!";

            IPlayableCharacter playableActor = Actor as IPlayableCharacter;
            INonPlayableCharacter nonPlayableActor = Actor as INonPlayableCharacter;
            if (Whom.Fighting != null)
            {
                // if not in same group, don't allow kill stealing
                bool isInSameGroup = playableActor != null && Whom.Fighting is IPlayableCharacter fightingPlayableCharacter && playableActor.IsSameGroup(fightingPlayableCharacter);
                if (!isInSameGroup)
                    return "Kill stealing is not permitted.";
            }

            if (Actor.CharacterFlags.HasFlag(CharacterFlags.Charm) && nonPlayableActor?.Master == Whom)
                return Actor.ActPhrase("{0:N} is your beloved master.", Whom);

            if (Actor.Position == Positions.Fighting)
                return "You do the best you can!";

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            // GCD
            (Actor as IPlayableCharacter)?.ImpersonatedBy?.SetGlobalCooldown(1);
            //TODO: check_killer( ch, victim );

            // Starts fight
            Actor.MultiHit(Whom);
        }
    }
}
