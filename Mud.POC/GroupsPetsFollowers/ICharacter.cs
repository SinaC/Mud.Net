﻿using System.Text;
using Mud.Server.Input;

namespace Mud.POC.GroupsPetsFollowers
{
    public interface ICharacter
    {
        bool IsValid { get; }

        IRoom Room { get; }

        string Name { get; }
        string DisplayName { get; }
        int Level { get; }
        int HitPoints { get; }
        int MaxHitPoints { get; }

        void ChangeRoom(IRoom room);

        ICharacter Leader { get; } // character we are following, different from group leader

        void AddFollower(ICharacter character);
        void RemoveFollower(ICharacter character);
        void ChangeLeader(ICharacter character);

        void Send(string format, params object[] args);
        void Send(StringBuilder sb);
        void Act(ActTargets target, string format, params object[] args);

        void OnRemoved();

        // TEST PURPOSE
        IWorld World { get; }
        CommandExecutionResults DoFollow(string rawParameters, params CommandParameter[] parameters);
        CommandExecutionResults DoNofollow(string rawParameters, params CommandParameter[] parameters);
    }

    public enum ActTargets
    {
        ToCharacter,
        ToRoom,

        ToGroup, // every in the group including 'this'
    }
}