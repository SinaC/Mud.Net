using Mud.Server.Common;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Mud.POC.Groups
{
    public abstract class CharacterBase : ICharacter
    {
        private List<ICharacter> _followers;

        public CharacterBase(string name, IRoom room)
        {
            _followers = new List<ICharacter>();

            Name = name;
            Room = room;
            Room.Enter(this);
        }

        #region ICharacter

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

        public IEnumerable<ICharacter> Followers => _followers;

        public ICharacter Follows { get; protected set; }

        public void AddFollower(ICharacter character)
        {
            if (Followers.Contains(character))
                return;
            if (character.Follows != null)
                character.Follows.RemoveFollower(character);
            _followers.Add(character);
            character.ChangeFollows(this);
            Act(ActTargets.ToCharacter, "{0:N} starts following you.");
            character.Act(ActTargets.ToCharacter, "You start following {0:N}.", this);
        }

        public void RemoveFollower(ICharacter character)
        {
            if (!Followers.Contains(character))
                return;
            Act(ActTargets.ToCharacter, "{0:N} stops following you.");
            character.Act(ActTargets.ToCharacter, "You stop following {0:N}.", this);
            _followers.Remove(character);
            character.ChangeFollows(null);
        }

        public void ChangeFollows(ICharacter character)
        {
            Follows = character;
        }

        public void Send(string format, params object[] args)
        {
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
            Debug.Write(sb.ToString());
        }

        public void Act(ActTargets actTarget, string format, params object[] args)
        {
            IEnumerable<ICharacter> targets;
            if (actTarget == ActTargets.ToCharacter)
                targets = this.Yield();
            else if (actTarget == ActTargets.ToGroup && this is IPlayableCharacter playableCharacter)
                targets = playableCharacter?.Group.Members ?? playableCharacter.Yield();
            else
                throw new Exception($"Invalid ActTargets {actTarget}");
            foreach (ICharacter target in targets)
                target.Send(format+string.Join(",",args.Select(x => $"[{x}]"))); // TODO: formatting done in real implementation
        }

        public virtual void OnRemoved()
        {
            // Leave follower
            Follows?.RemoveFollower(this);

            // Release followers
            foreach (ICharacter follower in _followers)
                follower.ChangeFollows(null);
            _followers.Clear();
        }

        #endregion

        public override string ToString() => DisplayName;
    }
}
