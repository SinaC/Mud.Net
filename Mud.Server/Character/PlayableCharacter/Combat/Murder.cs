using Mud.Domain;
using Mud.Logger;
using Mud.Server.Character.Communication;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Server.Character.PlayableCharacter.Combat
{
    [PlayableCharacterCommand("murder", "Combat", Priority = 999/*low priority*/, NoShortcut = true, MinPosition = Positions.Fighting)]
    [Syntax("[cmd] <character>")]
    public class Murder : CharacterGameAction
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

            IPlayableCharacter playableActor = Actor as IPlayableCharacter;
            if (Whom.Fighting != null)
            {
                // if not in same group, don't allow kill stealing
                bool isInSameGroup = playableActor != null && Whom.Fighting is IPlayableCharacter fightingPlayableCharacter && playableActor.IsSameGroup(fightingPlayableCharacter);
                if (!isInSameGroup)
                    return "Kill stealing is not permitted.";
            }

            INonPlayableCharacter nonPlayableActor = Actor as INonPlayableCharacter;
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

            string msg = $"Help! I am being attacked by {Actor.DisplayName}!";
            Yell yell = new Yell();
            ICharacterGameActionInfo yellGameActionInfo = CharacterGameActionInfo.Create(typeof(Yell));
            IActionInput yellActionInput = new ActionInput(yellGameActionInfo, Whom, null/*TODO*/, "yell", msg, new CommandParameter(msg, false));
            string yellGuards = yell.Guards(yellActionInput);
            if (yellGuards != null)
                Log.Default.WriteLine(LogLevels.Error, "Murder: Yell.Guards returned {0}", yellGuards);
            else
                yell.Execute(yellActionInput);

            // Starts fight
            Actor.MultiHit(Whom);
        }
    }
}
