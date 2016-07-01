using System;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Character
{
    public partial class Character
    {
        [Command("kill", Category = "Combat")]
        protected virtual bool DoKill(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Kill whom?" + Environment.NewLine);
                return true;
            }

            ICharacter target = FindHelpers.FindByName(Room.People, parameters[0]);
            if (target == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return true;
            }

            if (target == this)
            {
                Send("You hit yourself. Ouch!" + Environment.NewLine);
                return true;
            }

            if (target.Impersonable)
            {
                Send("You must MURDER a player!" + Environment.NewLine);
                return true;
            }

            // TODO
            //if (is_safe(ch, victim))
            //    return;

            //if (victim->fighting != NULL &&
            //     !is_same_group(ch, victim->fighting))
            //{
            //    send_to_char("Kill stealing is not permitted.\n\r", ch);
            //    return;
            //}

            //if (IS_AFFECTED(ch, AFF_CHARM) && ch->master == victim)
            //{
            //    act("$N is your beloved master.", ch, NULL, victim, TO_CHAR);
            //    return;
            //}

            //if (ch->position == POS_FIGHTING)
            //{
            //    send_to_char("You do the best you can!\n\r", ch);
            //    return;
            //}

            ImpersonatedBy?.SetGlobalCooldown(1);
            //TODO: check_killer( ch, victim );

            MultiHit(target);
            return true;
        }
    }
}
