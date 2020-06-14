using System.Linq;
using Mud.Server.Input;
using Mud.Common;

namespace Mud.POC.GroupsPetsFollowers
{
    public partial class CharacterBase
    {
        [CharacterCommand("Follow", "Group", "Follow")]
        [Syntax(
            "[cmd]",
            "[cmd] <character>")]
        public CommandExecutionResults DoFollow(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                if (Leader == null)
                {
                    Send("You are not following anyone.");
                    return CommandExecutionResults.NoExecution;
                }
                Act(ActOptions.ToCharacter, "You are following {0:N}.", Leader);
                return CommandExecutionResults.Ok;
            }

            // search target
            ICharacter target = Room.People.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, parameters[0].Value)); // TODO: use FindHelpers
            if (target == null)
            {
                Send("They aren't here.");
                return CommandExecutionResults.TargetNotFound;
            }

            // follow ourself -> cancel follow
            if (target == this)
            {
                if (Leader == null)
                {
                    Send("You are not following anyone.");
                    return CommandExecutionResults.InvalidTarget;
                }

                Leader.RemoveFollower(this);
                return CommandExecutionResults.Ok;
            }

            // check cycle
            ICharacter next = Leader;
            while (next != null)
            {
                if (next == target)
                {
                    Act(ActOptions.ToCharacter, "You can't follow {0:N}.", target);
                    return CommandExecutionResults.InvalidTarget; // found a cycle
                }
                next = next.Leader;
            }

            target.AddFollower(this);
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("Nofollow", "Group", "Follow")]
        [Syntax("[cmd]")]
        public CommandExecutionResults DoNofollow(string rawParameters, params CommandParameter[] parameters)
        {
            foreach (ICharacter follower in World.Characters.Where(x => x.Leader == this))
                RemoveFollower(follower);
            return CommandExecutionResults.Ok;
        }
    }
}
