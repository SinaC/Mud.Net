using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mud.Server.Common;
using Mud.Server.Input;

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
                if (Follows == null)
                {
                    Send("You are not following anyone.");
                    return CommandExecutionResults.NoExecution;
                }
                Act(ActTargets.ToCharacter, "You are following {0:N}.", Follows);
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
                if (Follows == null)
                {
                    Send("You are not following anyone.");
                    return CommandExecutionResults.InvalidTarget;
                }

                Follows.RemoveFollower(this);
                return CommandExecutionResults.Ok;
            }

            // check cycle
            ICharacter next = Follows;
            while (next != null)
            {
                if (next == target)
                {
                    Act(ActTargets.ToCharacter, "You can't follow {0:N}.", target);
                    return CommandExecutionResults.InvalidTarget; // found a cycle
                }
                next = next.Follows;
            }

            target.AddFollower(this);
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("Nofollow", "Group", "Follow")]
        [Syntax("[cmd]")]
        public CommandExecutionResults DoNofollow(string rawParameters, params CommandParameter[] parameters)
        {
            if (Followers.Any())
            {
                IReadOnlyCollection<ICharacter> clone = new ReadOnlyCollection<ICharacter>(Followers.ToList());
                foreach (ICharacter follower in clone)
                    RemoveFollower(follower);
                return CommandExecutionResults.Ok;
            }
            Send("You don't have any followers.");
            return CommandExecutionResults.NoExecution;
        }
    }
}
