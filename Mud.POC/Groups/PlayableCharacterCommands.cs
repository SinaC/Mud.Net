using Mud.Server.Common;
using Mud.Server.Input;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Mud.POC.Groups
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

            if (Pets.Count() == 0)
            {
                Send("You don't have any pets");
                return CommandExecutionResults.NoExecution;
            }

            // Get pets
            IEnumerable<INonPlayableCharacter> targets;
            if (parameters[0].IsAll)
                targets = Pets;
            else
                targets = Pets.First().Yield();

            // Remove pet name or all from parameters
            (string rawParameters, CommandParameter[] parameters) skip1 = CommandHelpers.SkipParameters(parameters, 1);

            // Send the order to selected pets
            foreach (INonPlayableCharacter target in targets)
                target.Order(skip1.rawParameters, skip1.parameters);

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
                // not in a group and no pet
                if (Group == null && !Pets.Any())
                {
                    Send("You aren't in a group.");
                    return CommandExecutionResults.NoExecution;
                }
                // not in a group but pets
                StringBuilder sb = new StringBuilder();
                if (Group == null && Pets.Any())
                {
                    // display pet info
                    foreach (INonPlayableCharacter pet in Pets)
                        sb.AppendFormatLine("{0,10}: Lvl: {1,3} Hp:{2,6}/{3,6}", pet.DisplayName.MaxLength(10), pet.Level, pet.HitPoints, pet.MaxHitPoints);
                }
                else
                {
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
                }

                Send(sb);

                return CommandExecutionResults.Ok;
            }

            // add member to group
            IPlayableCharacter target = Room.PlayableCharacters.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, parameters[0].Value)); // TODO: use FindHelpers
            if (target == null)
            {
                Send("They arent here.");
                return CommandExecutionResults.TargetNotFound;
            }
            if (target.Group != null)
            {
                Act(ActTargets.ToCharacter, "{0:N} is already in a group.");
                return CommandExecutionResults.InvalidTarget;
            }
            // create a new group if not already in a group
            if (Group == null)
                Group = new Group(this);
            // if in a group, we must be the leader
            else if (Group.Leader != this)
            {
                Send("You are not the group leader.");
                return CommandExecutionResults.NoExecution;
            }

            // add target in the group
            Group.AddMember(target);

            return CommandExecutionResults.Ok;
        }

        [Command("Ungroup", "Group")]
        [Syntax("[cmd] <member>")]
        public CommandExecutionResults DoUngroup(string rawParameters, params CommandParameter[] parameters) // remove member
        {
            if (parameters.Length == 0)
            {
                Send("Ungroup whom?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            if (Group == null)
            {
                Send("You aren't in a group.");
                return CommandExecutionResults.NoExecution;
            }
            // search member
            IPlayableCharacter member = Group.Members.FirstOrDefault(x => x != this && StringCompareHelpers.StringStartsWith(x.Name, parameters[0].Value)); // TODO: use FindHelpers
            if (member == null)
            {
                Send("There is not member of that name.");
                return CommandExecutionResults.TargetNotFound;
            }
            // remove from group
            Group.RemoveMember(member);
            if (Group.Members.Count() <= 1) // we are the last member -> delete group
            {
                Group.RemoveMember(this);
                Group = null;
            }

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
            Group.RemoveMember(this);
            Group = null;

            return CommandExecutionResults.Ok;
        }
    }
}
