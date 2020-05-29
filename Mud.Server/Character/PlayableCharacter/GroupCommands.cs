using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Input;
// ReSharper disable UnusedMember.Global

namespace Mud.Server.Character.PlayableCharacter
{
    public partial class PlayableCharacter
    {
        [PlayableCharacterCommand("order", "Pets")]
        [Syntax(
            "[cmd] <pet|charmie> command",
            "[cmd] all command")]
        protected virtual CommandExecutionResults DoOrder(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length < 2)
            {
                Send("Order whom to do what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            // Select target(s)
            IEnumerable<INonPlayableCharacter> targets;
            if (parameters[0].IsAll)
                targets = Room.NonPlayableCharacters.Where(x => x.Master == this && x.CharacterFlags.HasFlag(Domain.CharacterFlags.Charm));
            else
            {
                INonPlayableCharacter target = FindHelpers.FindByName(Room.NonPlayableCharacters.Where(CanSee), parameters[0]);
                if (target == null)
                {
                    Send(StringHelpers.CharacterNotFound);
                    return CommandExecutionResults.TargetNotFound;
                }

                if (target.Master != this || !target.CharacterFlags.HasFlag(Domain.CharacterFlags.Charm))
                {
                    Send("Do it yourself!");
                    return CommandExecutionResults.InvalidTarget;
                }

                targets = target.Yield();
            }

            // Remove target name or all from parameters
            var (modifiedRawParameters, modifiedParameters) = CommandHelpers.SkipParameters(parameters, 1);

            // Send the order to selected targets
            bool found = false;
            IReadOnlyCollection<INonPlayableCharacter> clone = new ReadOnlyCollection<INonPlayableCharacter>(targets.ToList());
            foreach (INonPlayableCharacter target in clone)
            {
                Act(ActOptions.ToCharacter, "You order {0:N} to '{1}'.", target, modifiedRawParameters);
                target.Order(modifiedRawParameters, modifiedParameters);
                found = true;
            }

            if (found)
            {
                Send("Ok.");
                ImpersonatedBy?.SetGlobalCooldown(Pulse.PulseViolence);
            }
            else
                Send("You don't have followers here.");

            return CommandExecutionResults.Ok;
        }

        [Command("group", "Group", "Information")]
        [Syntax(
            "[cmd]",
            "[cmd] <character>")]
        protected virtual CommandExecutionResults DoGroup(string rawParameters, params CommandParameter[] parameters)
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
                        AppendPetGroupMemberInfo(sbPets, pet);
                    Send(sbPets);

                    return CommandExecutionResults.Ok;
                }
                StringBuilder sb = new StringBuilder();
                sb.AppendFormatLine("{0}'s group:", Group.Leader.DisplayName);
                foreach (IPlayableCharacter member in Group.Members)
                {
                    // display member info
                    AppendPlayerGroupMemberInfo(sb, member);
                    if (member.Pets.Any())
                    {
                        // display member's pet info
                        foreach (INonPlayableCharacter pet in Pets)
                            AppendPetGroupMemberInfo(sb, pet);
                    }
                }
                Send(sb);
                return CommandExecutionResults.Ok;
            }

            // add/remove member to group
            IPlayableCharacter target = FindHelpers.FindByName(Room.PlayableCharacters, parameters[0]);
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
                if (target.Group != null)
                {
                    Act(ActOptions.ToCharacter, "{0:N} is already in a group.");
                    return CommandExecutionResults.InvalidTarget;
                }
                // create a new group
                Group = new Group.Group(this);
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

        [Command("leave", "Group")]
        [Syntax("[cmd] <member>")]
        protected virtual CommandExecutionResults DoLeave(string rawParameters, params CommandParameter[] parameters)
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

        [PlayableCharacterCommand("gtell", "Group", "Communication")]
        [PlayableCharacterCommand("groupsay", "Group", "Communication", Priority = 1000)]
        [PlayableCharacterCommand("gsay", "Group", "Communication")]
        [Syntax("[cmd] <message>")]
        protected virtual CommandExecutionResults DoGroupSay(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Say your group what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            Act(ActOptions.ToGroup, "%g%{0:n} says the group '%x%{1}%g%'%x%", this, parameters[0].Value);
            Send($"%g%You say to the group: '%x%{parameters[0].Value}%g%'%x%");
            return CommandExecutionResults.Ok;
        }

        //******************************************** Helpers ********************************************
        private void AppendPlayerGroupMemberInfo(StringBuilder sb, IPlayableCharacter member)
        {
            sb.AppendFormat("[{0,3} {1,3}] {2,20} {3,5}/{4,5} hp {5} {6,5}/{7,5} mv", member.Level, member.Class.ShortName, member.DisplayName.MaxLength(20), member.HitPoints, member.MaxHitPoints, BuildResources(member), member.MovePoints, member.MaxMovePoints);
            if (member.Level >= Settings.MaxLevel)
                sb.AppendFormat(" {0} nxt", member.ExperienceToLevel);
            sb.AppendLine();
        }

        private void AppendPetGroupMemberInfo(StringBuilder sb, INonPlayableCharacter member)
        {
            sb.AppendFormatLine("[{0,3} Pet] {1,20} {2,5}/{3,5} hp {4} {5,5}/{6,5} mv", member.Level, member.DisplayName.MaxLength(20), member.HitPoints, member.MaxHitPoints, BuildResources(member), member.MovePoints, member.MaxMovePoints);
        }

        private StringBuilder BuildResources(ICharacter character)
        {
            StringBuilder sb = new StringBuilder();
            foreach (ResourceKinds resource in character.CurrentResourceKinds)
                sb.AppendFormat("{0,5}/{1,5} {2}", character[resource], character.MaxResource(resource), resource.ToString().ToLowerInvariant());
            return sb;
        }
    }
}
