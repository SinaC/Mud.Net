using Mud.POC.Abilities2.Domain;

namespace Mud.POC.Abilities2.ExistingCode
{
    public interface INonPlayableCharacter : ICharacter
    {
        IPlayableCharacter Master { get; }

        ActFlags ActFlags { get; }
    }
}
