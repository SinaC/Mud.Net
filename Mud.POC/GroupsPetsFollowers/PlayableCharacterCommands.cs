using Mud.Server.Common;
using Mud.Server.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mud.POC.GroupsPetsFollowers
{
    public partial class PlayableCharacter
    {
        [PlayableCharacterCommand("Order", "Pets")]
        [Syntax(
            "[cmd] <pet> command",
            "[cmd] all command")]
        public CommandExecutionResults DoOrder(string rawParameters, params CommandParameter[] parameters) // order to one pet or all to do something
        {
            if (parameters.Length < 2)
            {
                Send("Order who what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            if (!Pets.Any())
            {
                Send("You don't have any pets");
                return CommandExecutionResults.NoExecution;
            }

            // Get pet(s)
            IEnumerable<INonPlayableCharacter> targets;
            if (parameters[0].IsAll)
                targets = Pets;
            else
            {
                INonPlayableCharacter pet = Pets.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, parameters[0].Value)); // TODO: use FindHelpers
                if (pet == null)
                {
                    Send("You don't have any pet of that name.");
                    return CommandExecutionResults.TargetNotFound;
                }

                targets = pet.Yield();
            }

            // Remove pet name or all from parameters
            var (modifiedRawParameters, modifiedParameters) = CommandHelpers.SkipParameters(parameters, 1);

            // Send the order to selected pets
            foreach (INonPlayableCharacter target in targets)
                target.Order(modifiedRawParameters, modifiedParameters);

            return CommandExecutionResults.Ok;
        }

        [Command("Group", "Group", "Information")]
        [Syntax(
            "[cmd]",
            "[cmd] <character>")]
        public CommandExecutionResults DoGroup(string rawParameters, params CommandParameter[] parameters) // display group info, add member
        {
            // no parameter: display group/pets info
            if (parameters.Length == 0)
            {
                if (Group == null)
                {
                    // not in a group and no pet
                    if (!Pets.Any())
                    {
                        Send("You aren't in a group.");
                        return CommandExecutionResults.NoExecution;
                    }
                    // not in a group but pets
                    StringBuilder sbPets = new StringBuilder();
                    foreach (INonPlayableCharacter pet in Pets)
                        sbPets.AppendFormatLine("{0,10}: Lvl: {1,3} Hp:{2,6}/{3,6}", pet.DisplayName.MaxLength(10), pet.Level, pet.HitPoints, pet.MaxHitPoints);
                    Send(sbPets);

                    return CommandExecutionResults.Ok;
                }
                StringBuilder sb = new StringBuilder();
                foreach (IPlayableCharacter member in Group.Members)
                {
                    // display member info
                    sb.AppendFormatLine("{0,3}{1,10}: Lvl: {2,3} Hp:{3,6}/{4,6} Nxt: {5,6}", Group.Leader == member ? "[L]" : string.Empty, member.DisplayName.MaxLength(10), member.Level, member.HitPoints, member.MaxHitPoints, member.ExperienceToLevel);
                    if (member.Pets.Any())
                    {
                        // display member's pet info
                        foreach (INonPlayableCharacter pet in Pets)
                            sb.AppendFormatLine("    {0,10}: Lvl: {1,3} Hp:{2,6}/{3,6}", pet.DisplayName.MaxLength(10), pet.Level, pet.HitPoints, pet.MaxHitPoints);
                    }
                }
                Send(sb);
                return CommandExecutionResults.Ok;
            }

            // add/remove member to group
            IPlayableCharacter target = Room.PlayableCharacters.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, parameters[0].Value)); // TODO: use FindHelpers
            if (target == null)
            {
                Send("They aren't here.");
                return CommandExecutionResults.TargetNotFound;
            }

            // can't group ourself
            if (target == this)
            {
                Send("You can't group yourself.");
                return CommandExecutionResults.InvalidTarget;
            }

            // we are not in a group -> add
            if (Group == null)
            {
                if(target.Group != null)
                {
                    Act(ActOptions.ToCharacter, "{0:N} is already in a group.");
                    return CommandExecutionResults.InvalidTarget;
                }
                // create a new group
                Group = new Group(this);
                // add target in the group
                Group.AddMember(target);
                return CommandExecutionResults.Ok;
            }
            // we are in a group -> add or remove
            // only the leader can add or remove
            if (Group.Leader != this)
            {
                Send("You are not the group leader.");
                return CommandExecutionResults.NoExecution;
            }
            // if already in a group -> remove or nop
            if (target.Group != null)
            {
                // not in the same group
                if (target.Group != Group)
                {
                    Act(ActOptions.ToCharacter, "{0:N} is already in a group.");
                    return CommandExecutionResults.InvalidTarget;
                }
                // remove target from the group or disband
                if (Group.Members.Count() <= 2) // group will contain only one member, disband
                    Group.Disband();
                else
                    Group.RemoveMember(target); // simple remove
                return CommandExecutionResults.Ok;
            }

            // add target in the group
            Group.AddMember(target);
            return CommandExecutionResults.Ok;
        }

        [Command("Leave", "Group")]
        [Syntax("[cmd] <member>")]
        public CommandExecutionResults DoLeave(string rawParameters, params CommandParameter[] parameters) // leave a group
        {
            if (Group == null)
            {
                Send("You aren't in a group.");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            if (Group.Members.Count() <= 2) // group will contain only one member, disband
               Group.Disband();
            else
                Group.RemoveMember(this);

            return CommandExecutionResults.Ok;
        }
    }
}
