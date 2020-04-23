﻿using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Character
{
    public partial class CharacterBase
    {
        [Command("kill", Category = "Combat")]
        [Syntax("[cmd] <character>")]
        protected virtual CommandExecutionResults DoKill(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Kill whom?");
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

            if (target is IPlayableCharacter)
            {
                Send("You must MURDER a player!");
                return CommandExecutionResults.InvalidTarget;
            }

            // TODO
            //if (is_safe(ch, victim))
            //    return;

            IPlayableCharacter playableCharacter = this as IPlayableCharacter;
            if (target.Fighting != null)
            {
                // if not in same group, don't allow kill stealing
                bool isInSameGroup = playableCharacter != null && target.Fighting is IPlayableCharacter fightingPlayableCharacter && playableCharacter.IsSameGroup(fightingPlayableCharacter);
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

            playableCharacter?.ImpersonatedBy?.SetGlobalCooldown(1);
            //TODO: check_killer( ch, victim );

            // Starts fight
            MultiHit(target);
            return CommandExecutionResults.Ok;
        }

        [Command("flee", Category = "Combat")]
        protected virtual CommandExecutionResults DoFlee(string rawParameters, params CommandParameter[] parameters)
        {
            if (Fighting == null)
            {
                Send("You aren't fighting anyone.");
                return CommandExecutionResults.NoExecution;
            }
            IRoom from = Room;

            // Try 6 times to find an exit
            for (int attempt = 0; attempt < 6; attempt++)
            {
                int randomExit = RandomManager.Randomizer.Next(ExitDirectionsExtensions.ExitCount);
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
                        Send("You flee from combat!");
                        Act(ActOptions.ToRoom, "{0} has fled!", this);
                        return CommandExecutionResults.Ok;
                    }
                }
            }

            Send("PANIC! You couldn't escape!");
            return CommandExecutionResults.Ok;
        }
    }
}
