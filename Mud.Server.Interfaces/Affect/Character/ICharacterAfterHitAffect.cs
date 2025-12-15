using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Affect.Character
{
    public interface ICharacterAfterHitAffect : IAffect
    {
        void AfterHit(ICharacter hitter, ICharacter victim);
    }
}
