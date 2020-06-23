using Mud.Domain;
using Mud.Server.Character.Communication;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.PlayableCharacter.Combat
{
    [PlayableCharacterCommand("murder", "Combat", Priority = 999/*low priority*/, NoShortcut = true, MinPosition = Positions.Fighting)]
    [Syntax("[cmd] <character>")]
    public class Murder : PlayableCharacterGameAction
    {
        private IGameActionManager GameActionManager { get; }

        public ICharacter Whom { get; protected set; }

        public Murder(IGameActionManager gameActionManager)
        {
            GameActionManager = gameActionManager;
        }

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

            string safeResult = Whom.IsSafe(Actor);
            if (safeResult != null)
                return safeResult;

            IPlayableCharacter playableActor = Actor;
            if (Whom.Fighting != null)
            {
                // if not in same group, don't allow kill stealing
                bool isInSameGroup = playableActor != null && Whom.Fighting is IPlayableCharacter fightingPlayableCharacter && playableActor.IsSameGroup(fightingPlayableCharacter);
                if (!isInSameGroup)
                    return "Kill stealing is not permitted.";
            }

            // this is impossible with new game action system
            //INonPlayableCharacter nonPlayableActor = Actor as INonPlayableCharacter;
            //if (Actor.CharacterFlags.HasFlag(CharacterFlags.Charm) && nonPlayableActor?.Master == Whom)
            //    return Actor.ActPhrase("{0:N} is your beloved master.", Whom);

            if (Actor.Position == Positions.Fighting)
                return "You do the best you can!";

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            // GCD
            Actor.ImpersonatedBy?.SetGlobalCooldown(1);
            //TODO: check_killer( ch, victim );

            string msg = $"Help! I am being attacked by {Actor.DisplayName}!";

            string executionResults = GameActionManager.Execute<Yell, ICharacter>(Whom, msg);
            if (executionResults != null)
                Actor.Send(executionResults);

            // Starts fight
            Actor.MultiHit(Whom);
        }
    }
}
