using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Server.Constants;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Character
{
    public partial class Character
    {
        [Command("follow")]
        protected virtual bool DoFollow(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Follow whom?"+Environment.NewLine);
                return true;
            }
            ICharacter victim = FindHelpers.FindByName(Room.People.Where(CanSee), parameters[0]);
            if (victim == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return true;
            }
            // TODO: charmed ?
            if (victim == this)
            {
                if (Leader == null)
                {
                    Send("You already follow yourself."+Environment.NewLine);
                    return true;
                }
                //TODO: StopFollower();
                return true;
            }

            if (Leader != null)
                //StopFollower(); TODO
                ;
            //TODO: AddFollower(victim);
            return true;
        }

        // TODO: we should not be able to group someone who doesn't want to be grouped
        //  solution: 2 commands: follow + group (can only group someone following)
        [Command("group")]
        protected virtual bool DoGroup(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                ICharacter leader = Leader ?? this; // TODO: don't display group information if no the same group as leader
                StringBuilder sb = new StringBuilder();
                sb.AppendFormatLine("{0}'s group:", leader.DisplayName);
                AppendCharacterGroupMemberInfo(sb, leader);
                foreach (ICharacter member in leader.GroupMembers)
                    AppendCharacterGroupMemberInfo(sb, member);
                Send(sb);
            }
            else
            {
                if (Leader != null)
                {
                    Send("You are not the group leader." + Environment.NewLine);
                    return true;
                }
                // Remove from group if target is already in the group
                ICharacter oldMember = FindHelpers.FindByName(GroupMembers.Where(CanSee), parameters[0]);
                if (oldMember != null)
                {
                    RemoveGroupMember(oldMember); // TODO: what if leader leaves group!!!
                    return true;
                }
                // Search new member to add
                ICharacter newMember = FindHelpers.FindByName(Room.People.Where(CanSee), parameters[0]);
                if (newMember == null) // not found
                {
                    Send(StringHelpers.CharacterNotFound);
                    return true;
                }
                if (newMember == this)
                {
                    Send("You cannot group yourself."+Environment.NewLine);
                    return true;
                }
                if (newMember.Leader != null) // already in a group
                {
                    Send("But {0} is following someone else." + Environment.NewLine, newMember.DisplayName);
                    return true;
                }
                AddGroupMember(newMember);
            }
            return true;
        }

        [Command("gtell")]
        [Command("gsay")]
        protected virtual bool DoGroupSay(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Say your group what?" + Environment.NewLine);
                return true;
            }
            Send("%g%You say the group '%w%{0}%g%'%x%", rawParameters);
            IReadOnlyCollection<ICharacter> members = Leader == null ? GroupMembers : Leader.GroupMembers;
            foreach (ICharacter member in members)
                member.Act(ActOptions.ToCharacter, "%g%{0} says the group'%w%{1}%g%'%x%", this, rawParameters);
            return true;
        }

        //******************************************** Helpers ********************************************
        private void AppendCharacterGroupMemberInfo(StringBuilder sb, ICharacter member)
        {
            // TODO: add class, mana, xp, ...
            sb.AppendFormatLine("[{0,3}] {1,-16} {2,5}/{3,5}hp", member.Level, member.DisplayName, member.HitPoints, member.GetComputedAttribute(ComputedAttributeTypes.MaxHitPoints));
        }
    }
}
