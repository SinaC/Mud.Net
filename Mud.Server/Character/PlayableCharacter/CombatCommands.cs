using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
// ReSharper disable UnusedMember.Global

namespace Mud.Server.Character.PlayableCharacter
{
    public partial class PlayableCharacter
    {
        [PlayableCharacterCommand("murder", "Combat", Priority = 999/*low priority*/, NoShortcut = true, MinPosition = Positions.Fighting)]
        [Syntax("[cmd] <character>")]
        protected virtual CommandExecutionResults DoMurder(string rawParameters, params ICommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Murder whom?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            ICharacter target = FindHelpers.FindByName(Room.People, parameters[0]);
            if (target == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return CommandExecutionResults.TargetNotFound;
            }

            if (target == this)
            {
                Send("You hit yourself. Ouch!");
                return CommandExecutionResults.InvalidTarget;
            }

            // TODO
            //if (is_safe(ch, victim))
            //    return;

            if (target.Fighting != null)
            {
                bool isInSameGroup = this is IPlayableCharacter playableCharacter && target.Fighting is IPlayableCharacter fightingPlayableCharacter && playableCharacter.IsSameGroup(fightingPlayableCharacter);
                if (!isInSameGroup)
                {
                    Send("Kill stealing is not permitted.");
                    return CommandExecutionResults.InvalidTarget;
                }
            }

            //if (IS_AFFECTED(ch, AFF_CHARM) && ch->master == victim)
            //{
            //    act("$N is your beloved master.", ch, NULL, victim, TO_CHAR);
            //    return;
            //}

            if (Position == Positions.Fighting)
            {
                Send("You do the best you can!");
                return CommandExecutionResults.NoExecution;
            }

            ImpersonatedBy?.SetGlobalCooldown(1);
            //TODO: check_killer( ch, victim );

            // Starts fight
            MultiHit(target);
            return CommandExecutionResults.Ok;
        }
    }
}
