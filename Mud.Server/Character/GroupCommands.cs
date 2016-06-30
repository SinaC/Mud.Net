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
        [Command("follow", Category = "Group")]
        protected virtual bool DoFollow(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Follow whom?"+Environment.NewLine);
                return true;
            }
            ICharacter newLeader = FindHelpers.FindByName(Room.People.Where(CanSee), parameters[0]);
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
                    Send("You already follow yourself."+Environment.NewLine);
                    return true;
                }
                Leader.StopFollower(this);
                return true;
            }

            Leader?.StopFollower(this);
            newLeader.AddFollower(this);
            return true;
        }

        [Command("group", Category = "Group")]
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
                // Search in room a new member to add
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
                if (newMember.Leader != this)
                {
                    Act(ActOptions.ToCharacter, "{0} is not following you.", newMember);
                    return true;
                }
                if (GroupMembers.Any(x => x == newMember))
                {
                    Act(ActOptions.ToCharacter, "{0} is already in your group.", newMember);
                    return true;
                }
                AddGroupMember(newMember);
            }
            return true;
        }

        [Command("gtell", Category = "Group")] // TODO: multiple category +Communication
        [Command("gsay", Category = "Group")] // TODO: multiple category +Communication
        protected virtual bool DoGroupSay(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Say your group what?" + Environment.NewLine);
                return true;
            }
            Send("%g%You say the group '%W%{0}%g%'%x%", rawParameters);
            IReadOnlyCollection<ICharacter> members = Leader == null ? GroupMembers : Leader.GroupMembers;
            foreach (ICharacter member in members)
                member.Act(ActOptions.ToCharacter, "%g%{0} says the group'%W%{1}%g%'%x%", this, rawParameters);
            return true;
        }

        [Command("charm")] // TODO: remove   test commands
        protected virtual bool DoCharm(string rawParameters, params CommandParameter[] parameters)
        {
            if (ControlledBy != null)
                Send("You feel like taking, not giving, orders." + Environment.NewLine);
            else if (parameters.Length == 0)
            {
                if (Slave != null)
                {
                    Send("You stop controlling {0}." + Environment.NewLine, Slave.Name);
                    Slave.ChangeController(null);
                    Slave = null;
                }
                else
                    Send("Try controlling something before trying to un-control." + Environment.NewLine);
            }
            else
            {
                ICharacter target = FindHelpers.FindByName(Room.People, parameters[0]);
                if (target != null)
                {
                    if (target == this)
                        Send("You like yourself even better!" + Environment.NewLine);
                    else
                    {
                        target.ChangeController(this);
                        Slave = target;
                        Send("{0} looks at you with adoring eyes." + Environment.NewLine, target.Name);
                        target.Send("Isn't {0} so nice?" + Environment.NewLine, Name);
                    }
                }
                else
                    Send(StringHelpers.CharacterNotFound);
            }

            return true;
        }

        [Command("order", Category = "Group")]
        protected virtual bool DoOrder(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                Send("Order what?" + Environment.NewLine);
            else if (Slave == null)
                Send("You have no followers here." + Environment.NewLine);
            else if (Slave.Room != Room)
                Send(StringHelpers.CharacterNotFound);
            else
            {
                Slave.Send("{0} orders you to '{1}'." + Environment.NewLine, Name, rawParameters);
                Slave.ProcessCommand(rawParameters);
                SetGlobalCooldown(3);
                //Send("You order {0} to {1}." + Environment.NewLine, Slave.Name, rawParameters);
                Send("Ok." + Environment.NewLine);
            }
            return true;
        }

        //******************************************** Helpers ********************************************
        private void AppendCharacterGroupMemberInfo(StringBuilder sb, ICharacter member)
        {
            // TODO: add class, mana, xp, ...
            sb.AppendFormatLine("[{0,3}] {1,-16} {2,5}/{3,5}hp", member.Level, member.DisplayName, member.HitPoints, member[ComputedAttributeTypes.MaxHitPoints]);
        }
    }
}
