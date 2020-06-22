using System.Text;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

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
        void Act(ActOptions target, string format, params object[] args);

        void OnRemoved();

        // TEST PURPOSE
        IWorld World { get; }
        CommandExecutionResults DoFollow(string rawParameters, params ICommandParameter[] parameters);
        CommandExecutionResults DoNofollow(string rawParameters, params ICommandParameter[] parameters);
    }

    public enum ActOptions
    {
        ToCharacter,
        ToRoom,

        ToGroup, // every in the group including 'this'
    }
}
