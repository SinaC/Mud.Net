using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces
{
    public interface IHitAfterDamageManager
    {
        void OnHit(ICharacter hitter, ICharacter victim);
    }
}
