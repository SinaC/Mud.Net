﻿using System.Collections.Generic;
using System.Text;

namespace Mud.POC.Groups
{
    public interface ICharacter
    {
        IRoom Room { get; }

        string Name { get; }
        string DisplayName { get; }
        int Level { get; }
        int HitPoints { get; }
        int MaxHitPoints { get; }

        void ChangeRoom(IRoom room);

        IEnumerable<ICharacter> Followers { get; }
        ICharacter Follows { get; }

        void AddFollower(ICharacter character);
        void RemoveFollower(ICharacter character);
        void ChangeFollows(ICharacter character);

        void Send(string format, params object[] args);
        void Send(StringBuilder sb);
        void Act(ActTargets target, string format, params object[] args);

        void OnRemoved();
    }

    public enum ActTargets
    {
        ToCharacter,

        ToGroup, // every in the group including 'this'
    }
}
