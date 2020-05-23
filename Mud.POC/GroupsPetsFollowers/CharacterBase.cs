using Mud.Server.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Mud.POC.GroupsPetsFollowers
{
    public abstract partial class CharacterBase : ICharacter
    {
        protected CharacterBase(string name, IRoom room)
        {
            IsValid = true;
            Name = name;
            Room = room;
            Room.Enter(this);
            World.AddCharacter(this);
        }

        #region ICharacter

        public bool IsValid { get; protected set; }

        public IRoom Room { get; protected set; }

        public string Name { get; }

        public string DisplayName => Name;

        public int Level { get; protected set; }

        public int HitPoints { get; protected set; }

        public int MaxHitPoints { get; protected set; }

        public void ChangeRoom(IRoom room)
        {
            Room?.Leave(this);
            room?.Enter(this);
            Room = room;
        }

        public ICharacter Leader { get; protected set; }

        public void AddFollower(ICharacter character)
        {
            if (character.Leader == this)
                return;
            // check if A->B->C->A
            ICharacter next = Leader;
            while (next != null)
            {
                if (next == character)
                    return; // found a cycle
                next = next.Leader;
            }

            character.Leader?.RemoveFollower(character);
            character.ChangeLeader(this);
            Act(ActOptions.ToCharacter, "{0:N} starts following you.");
            character.Act(ActOptions.ToCharacter, "You start following {0:N}.", this);
        }

        public void RemoveFollower(ICharacter character)
        {
            if (character.Leader != this)
                return;
            Act(ActOptions.ToCharacter, "{0:N} stops following you.");
            character.Act(ActOptions.ToCharacter, "You stop following {0:N}.", this);
            character.ChangeLeader(null);
        }

        public void ChangeLeader(ICharacter character)
        {
            Leader = character;
        }

        public void Send(string format, params object[] args)
        {
            if (!IsValid)
                return;
            if (args.Length > 0)
            {
                string msg = $"[{DisplayName}]<-" + format + string.Join(",", args.Select(x => $"[{x}]"));
                Debug.WriteLine(msg, args);
            }
            else
            {
                string msg = $"[{DisplayName}]<-" + format;
                Debug.WriteLine(msg);
            }
        }

        public void Send(StringBuilder sb)
        {
            if (!IsValid)
                return;
            Debug.Write(sb.ToString());
        }

        public void Act(ActOptions actTarget, string format, params object[] args)
        {
            if (!IsValid)
                return;
            IEnumerable<ICharacter> targets;
            if (actTarget == ActOptions.ToCharacter)
                targets = this.Yield();
            else if (actTarget == ActOptions.ToRoom)
                targets = this.Room.People.Where(x => x != this);
            else if (actTarget == ActOptions.ToGroup && this is IPlayableCharacter playableCharacter)
                targets = playableCharacter.Group.Members ?? playableCharacter.Yield();
            else
                throw new Exception($"Invalid ActTargets {actTarget}");
            foreach (ICharacter target in targets)
                target.Send(format+string.Join(",",args.Select(x => $"[{x}]"))); // TODO: formatting done in real implementation
        }

        public virtual void OnRemoved()
        {
            IsValid = false;

            // Leave follower
            Leader?.RemoveFollower(this);

            // Release followers
            foreach (ICharacter follower in World.Characters.Where(x => x.Leader == this))
                RemoveFollower(follower);
        }

        // TEST PURPOSE
        public IWorld World => GroupsPetsFollowers.World.Instance;

        #endregion

        public override string ToString() => DisplayName;
    }
}
