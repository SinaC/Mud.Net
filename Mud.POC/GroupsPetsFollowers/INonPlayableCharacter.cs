using Mud.Server.Interfaces.GameAction;

namespace Mud.POC.GroupsPetsFollowers
{
    public interface INonPlayableCharacter : ICharacter
    {
        IPlayableCharacter Master { get; }

        void ChangeMaster(IPlayableCharacter master);

        void Order(string rawParameters, params ICommandParameter[] parameters);
    }
}
