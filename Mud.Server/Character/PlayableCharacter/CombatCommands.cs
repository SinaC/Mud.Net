using Mud.Domain;
using Mud.Server.Helpers;
using Mud.Server.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Server.Character.PlayableCharacter
{
    public partial class PlayableCharacter
    {
        [PlayableCharacterCommand("murder", Category = "Combat", Priority = 999/*low priority*/, NoShortcut = true)]
        protected virtual bool DoMurder(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Murder whom?");
                return true;
            }

            IPlayableCharacter target = FindHelpers.FindByName(Room.PlayableCharacters, parameters[0]);
            if (target == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return true;
            }

            if (target == this)
            {
                Send("You hit yourself. Ouch!");
                return true;
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
                    return true;
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
                return true;
            }

            ImpersonatedBy?.SetGlobalCooldown(1);
            //TODO: check_killer( ch, victim );

            // Starts fight
            MultiHit(target);
            return true;
        }
    }
}
