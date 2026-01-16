using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Affect.Character
{
    public interface ICharacterAdditionalHitAffect : IAffect
    {
        int AdditionalHitCount { get; set; }

        bool IsAdditionalHitAvailable(ICharacter character, int hitCount);
    }
}
