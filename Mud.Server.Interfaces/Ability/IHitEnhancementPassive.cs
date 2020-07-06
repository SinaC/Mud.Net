using Mud.Domain;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Ability
{
    public interface IHitEnhancementPassive : IPassive
    {
        int DamageModifier(ICharacter aggressor, ICharacter victim, SchoolTypes damageType, int baseDamage);
    }
}
