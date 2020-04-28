using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Character.PlayableCharacter
{
    public partial class PlayableCharacter
    {
        [PlayableCharacterCommand("follow", "Group")]
        [Syntax("[cmd] <character>")]
        protected virtual CommandExecutionResults DoFollow(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Follow whom?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            ICharacter whom = FindHelpers.FindByName(Room.People.Where(CanSee), parameters[0]);
            if (whom == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return CommandExecutionResults.TargetNotFound;
            }
            IPlayableCharacter newLeader = whom as IPlayableCharacter;
            if (newLeader == null)
            {
                Send($"You cannot follow {whom.DisplayName}");
                return CommandExecutionResults.InvalidTarget;
            }
            // TODO: charmed ?
            if (newLeader == this)
            {
                if (Leader == null)
                {
                    Send("You already follow yourself.");
                    return CommandExecutionResults.InvalidTarget;
                }
                Leader.StopFollower(this);
                return CommandExecutionResults.Ok;
            }

            Leader?.StopFollower(this);
            newLeader.AddFollower(this);
            return CommandExecutionResults.Ok;
        }

        [PlayableCharacterCommand("group", "Group")]
        [Syntax(
            "[cmd]",
            "[cmd] <character>")]
        protected virtual CommandExecutionResults DoGroup(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                IPlayableCharacter leader = null;
                // Member of a group
                if (Leader != null && Leader.GroupMembers.Any(x => x == this))
                    leader = Leader;
                // Leader of a group
                else if (Leader == null && GroupMembers.Any())
                    leader = this;
                if (leader == null)
                {
                    Send("You are not in a group.");
                    return CommandExecutionResults.NoExecution;
                }
                StringBuilder sb = new StringBuilder();
                sb.AppendFormatLine("{0}'s group:", leader.DisplayName); // No RelativeDisplayName because we always see people in our group
                AppendCharacterGroupMemberInfo(sb, leader, true);
                foreach (IPlayableCharacter member in leader.GroupMembers)
                    AppendCharacterGroupMemberInfo(sb, member, false);
                Send(sb);
                return CommandExecutionResults.Ok;
            }

            // Try to add/remove someone in group
            if (Leader != null)
            {
                Send("You are not the group leader.");
                return CommandExecutionResults.NoExecution;
            }
            // Remove from group if target is already in the group
            IPlayableCharacter oldMember = FindHelpers.FindByName(GroupMembers.Where(CanSee), parameters[0]);
            if (oldMember != null)
            {
                RemoveGroupMember(oldMember, false);
                return CommandExecutionResults.Ok;
            }
            // Search in room a new member to add
            ICharacter whom = FindHelpers.FindByName(Room.People.Where(CanSee), parameters[0]);
            if (whom == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return CommandExecutionResults.TargetNotFound;
            }
            IPlayableCharacter newMember = whom as IPlayableCharacter;
            if (newMember == null) // not found
            {
                Send($"You cannot group {whom.DisplayName}");
                return CommandExecutionResults.InvalidTarget;
            }
            if (newMember == this)
            {
                Send("You cannot group yourself.");
                return CommandExecutionResults.InvalidTarget;
            }
            if (newMember.Leader != this)
            {
                Act(ActOptions.ToCharacter, "{0} is not following you.", newMember);
                return CommandExecutionResults.InvalidTarget;
            }
            if (newMember.GroupMembers.Any())
            {
                Act(ActOptions.ToCharacter, "{0} is already in a group", newMember);
                return CommandExecutionResults.InvalidTarget;
            }
            if (GroupMembers.Any(x => x == newMember))
            {
                Act(ActOptions.ToCharacter, "{0} is already in your group.", newMember);
                return CommandExecutionResults.InvalidTarget;
            }
            AddGroupMember(newMember, false);
            return CommandExecutionResults.Ok;
        }

        [PlayableCharacterCommand("leave", "Group", Priority = 5)]
        protected virtual CommandExecutionResults DoLeave(string rawParameters, params CommandParameter[] parameters)
        {
            // Member leaving
            if (Leader != null && Leader.GroupMembers.Any(x => x == this))
                Leader.RemoveGroupMember(this, false);
            // Leader leaving -> change leader
            else if (GroupMembers.Any())
            {
                IPlayableCharacter newLeader = GroupMembers.FirstOrDefault();
                if (newLeader == null)
                {
                    Log.Default.WriteLine(LogLevels.Error, "DoLeave: problem with group, leader leaving but no other group member found.");
                    return CommandExecutionResults.Error;
                }
                // New leader has no leader
                newLeader.ChangeLeader(null);
                // Remove member from old leader and add it to new leader
                IReadOnlyCollection<IPlayableCharacter> members = new ReadOnlyCollection<IPlayableCharacter>(GroupMembers.Where(x => x != newLeader).ToList()); // clone because RemoveGroupMember will modify GroupMembers
                foreach (IPlayableCharacter member in members)
                {
                    RemoveGroupMember(member, true);
                    newLeader.AddGroupMember(member, true);
                }
                // Warn members about leader change
                newLeader.Send("You are the new group leader.");
                Act(ActOptions.ToGroup, "{0} is the new group leader.", newLeader);
            }
            else
                Send("You are not in a group.");
            return CommandExecutionResults.Ok;
        }

        [PlayableCharacterCommand("gtell", "Group", "Communication")]
        [PlayableCharacterCommand("groupsay", "Group", "Communication", Priority = 50)]
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
        private void AppendCharacterGroupMemberInfo(StringBuilder sb, IPlayableCharacter member, bool isLeader)
        {
            // TODO: add class, mana, xp, ...
            if (member.Level >= Settings.MaxLevel)
                sb.AppendFormatLine("[{0,3}]{1} {2,-30} {3,5}/{4,5}hp {5,5}/{6,5}Mv", member.Level, isLeader ? "L" : " ", member.DisplayName, member.HitPoints, member[SecondaryAttributeTypes.MaxHitPoints], member.MovePoints, member[SecondaryAttributeTypes.MaxMovePoints]);
            else
                sb.AppendFormatLine("[{0,3}]{1} {2,-30} {3,5}/{4,5}hp {5,5}/{6,5}Mv {7}Nxt", member.Level, isLeader ? "L" : " ", member.DisplayName, member.HitPoints, member[SecondaryAttributeTypes.MaxHitPoints], member.MovePoints, member[SecondaryAttributeTypes.MaxMovePoints], member.ExperienceToLevel);
        }
    }
}
