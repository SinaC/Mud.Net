using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Character
{
    public partial class CharacterBase
    {
        [CharacterCommand("kill", "Combat", Priority = 1, MinPosition = Positions.Fighting)]
        [Syntax("[cmd] <character>")]
        protected virtual CommandExecutionResults DoKill(string rawParameters, params ICommandParameter[] parameters)
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
                return CommandExecutionResults.InvalidTarget; // TODO: call MultiHit
            }

            if (target.IsSafe(this))
                return CommandExecutionResults.InvalidTarget;

            if (target is IPlayableCharacter)
            {
                Send("You must MURDER a player!");
                return CommandExecutionResults.InvalidTarget;
            }

            IPlayableCharacter playableCharacter = this as IPlayableCharacter;
            INonPlayableCharacter nonPlayableCharacter = this as INonPlayableCharacter;
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

            if (CharacterFlags.HasFlag(CharacterFlags.Charm) && nonPlayableCharacter?.Master == target)
            {
                Act(ActOptions.ToCharacter, "{0:N} is your beloved master.", target);
                return CommandExecutionResults.InvalidTarget;
            }

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

        [CharacterCommand("flee", "Combat", MinPosition = Positions.Standing)]
        protected virtual CommandExecutionResults DoFlee(string rawParameters, params ICommandParameter[] parameters)
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
                ExitDirections randomExit = RandomManager.Random<ExitDirections>();
                IExit exit = Room.Exits[(int)randomExit];
                IRoom destination = exit?.Destination;
                if (destination != null && !exit.IsClosed
                    && !(this is INonPlayableCharacter && destination.RoomFlags.HasFlag(RoomFlags.NoMob)))
                {
                    // Try to move without checking if in combat or not
                    Move(randomExit, false);
                    if (Room != from) // successful only if effectively moved away
                    {
                        //
                        StopFighting(true);
                        //
                        Send("You flee from combat!");
                        Act(from.People, "{0} has fled!", this);

                        if (this is IPlayableCharacter pc)
                        {
                            Send("You lost 10 exp.");
                            pc.GainExperience(-10);
                        }
                        return CommandExecutionResults.Ok;
                        // TODO: xp loss
                    }
                }
            }

            Send("PANIC! You couldn't escape!");
            return CommandExecutionResults.Ok;
        }
    }
}
