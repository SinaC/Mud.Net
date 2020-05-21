using Mud.Server.Input;

namespace Mud.POC.Groups
{
    public interface INonPlayableCharacter : ICharacter
    {
        IPlayableCharacter Master { get; }

        void ChangeMaster(IPlayableCharacter master);

        void Order(string rawParameters, params CommandParameter[] parameters);
    }
}
