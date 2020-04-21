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
        [PlayableCharacterCommand("follow", Category = "Group")]
        protected virtual bool DoFollow(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Follow whom?");
                return true;
            }
            IPlayableCharacter newLeader = FindHelpers.FindByName(Room.PlayableCharacters.Where(CanSee), parameters[0]);
            if (newLeader == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return true;
            }
            // TODO: charmed ?
            if (newLeader == this)
            {
                if (Leader == null)
                {
                    Send("You already follow yourself.");
                    return true;
                }
                Leader.StopFollower(this);
                return true;
            }

            Leader?.StopFollower(this);
            newLeader.AddFollower(this);
            return true;
        }

        [PlayableCharacterCommand("group", Category = "Group")]
        protected virtual bool DoGroup(string rawParameters, params CommandParameter[] parameters)
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
                    return true;
                }
                StringBuilder sb = new StringBuilder();
                sb.AppendFormatLine("{0}'s group:", leader.DisplayName);
                AppendCharacterGroupMemberInfo(sb, leader, true);
                foreach (IPlayableCharacter member in leader.GroupMembers)
                    AppendCharacterGroupMemberInfo(sb, member, false);
                Send(sb);
                return true;
            }

            // Try to add/remove someone in group
            if (Leader != null)
            {
                Send("You are not the group leader.");
                return true;
            }
            // Remove from group if target is already in the group
            IPlayableCharacter oldMember = FindHelpers.FindByName(GroupMembers.Where(CanSee), parameters[0]);
            if (oldMember != null)
            {
                RemoveGroupMember(oldMember, false);
                return true;
            }
            // Search in room a new member to add
            IPlayableCharacter newMember = FindHelpers.FindByName(Room.PlayableCharacters.Where(CanSee), parameters[0]);
            if (newMember == null) // not found
            {
                Send(StringHelpers.CharacterNotFound);
                return true;
            }
            if (newMember == this)
            {
                Send("You cannot group yourself.");
                return true;
            }
            if (newMember.Leader != this)
            {
                Act(ActOptions.ToCharacter, "{0} is not following you.", newMember);
                return true;
            }
            if (newMember.GroupMembers.Any())
            {
                Act(ActOptions.ToCharacter, "{0} is already in a group", newMember);
                return true;
            }
            if (GroupMembers.Any(x => x == newMember))
            {
                Act(ActOptions.ToCharacter, "{0} is already in your group.", newMember);
                return true;
            }
            AddGroupMember(newMember, false);
            return true;
        }

        [PlayableCharacterCommand("leave", Category = "Group", Priority = 5)]
        protected virtual bool DoLeave(string rawParameters, params CommandParameter[] parameters)
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
                    return true;
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
            return true;
        }

        [PlayableCharacterCommand("gtell", Category = "Group")] // TODO: multiple category +Communication
        [PlayableCharacterCommand("groupsay", Category = "Group", Priority = 50)]
        [PlayableCharacterCommand("gsay", Category = "Group")] // TODO: multiple category +Communication
        protected virtual bool DoGroupSay(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Say your group what?");
                return true;
            }
            Act(ActOptions.ToGroup, "%g%{0:n} says the group '%x%{1}%g%'%x%", this, parameters[0].Value);
            Send($"%g%You say to the group: '%x%{parameters[0].Value}%g%'%x%");
            return true;
        }

        //******************************************** Helpers ********************************************
        private void AppendCharacterGroupMemberInfo(StringBuilder sb, IPlayableCharacter member, bool isLeader)
        {
            // TODO: add class, mana, xp, ...
            if (member.Level >= Settings.MaxLevel)
                sb.AppendFormatLine("[{0,3}]{1} {2,-30} {3,5}/{4,5}hp", member.Level, isLeader ? "L" : " ", member.DisplayName, member.HitPoints, member[SecondaryAttributeTypes.MaxHitPoints]);
            else
                sb.AppendFormatLine("[{0,3}]{1} {2,-30} {3,5}/{4,5}hp {6}Nxt", member.Level, isLeader ? "L" : " ", member.DisplayName, member.HitPoints, member[SecondaryAttributeTypes.MaxHitPoints], member.ExperienceToLevel);
        }
    }
}
