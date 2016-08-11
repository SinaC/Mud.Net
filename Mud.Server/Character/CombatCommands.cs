using System;
using Mud.Server.Constants;
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

        [Command("murde", Hidden = true)] // TODO: force full match
        protected virtual bool DoMurde(string rawParameters, params CommandParameter[] parameters)
        {
            Send("If you want to MURDER, spell it out." + Environment.NewLine);
            return true;
        }

        [Command("murder", Category = "Combat")]
        protected virtual bool DoMurder(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Murder whom?" + Environment.NewLine);
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

        [Command("flee", Category = "Combat")]
        protected virtual bool DoFlee(string rawParameters, params CommandParameter[] parameters)
        {
            if (Fighting == null)
            {
                Send("You aren't fighting anyone." + Environment.NewLine);
                return true;
            }
            IRoom from = Room;

            bool successful = false;
            // Try 6 times to find an exit
            for (int attempt = 0; attempt < 6; attempt++)
            {
                int randomExit = RandomizeHelpers.Instance.Randomizer.Next(ExitHelpers.ExitCount);
                IRoom destination = Room.Exits[randomExit]?.Destination;
                if (destination != null)
                {
                    // Try to move without checking if in combat or not
                    Move((ExitDirections) randomExit, false);
                    if (Room != from) // successful only if effectively moved away
                    {
                        //
                        StopFighting(true);
                        //
                        Send("You flee from combat!"+Environment.NewLine);
                        Act(ActOptions.ToRoom, "{0} has fled!", this);
                        successful = true;
                        break;
                    }
                }
            }

            if (!successful)
                Send("PANIC! You couldn't escape!" + Environment.NewLine);
            return true;
        }
    }
}
