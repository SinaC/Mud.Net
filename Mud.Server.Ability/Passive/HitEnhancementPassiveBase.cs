using Mud.Domain;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Ability.Passive;

public abstract class HitEnhancementPassiveBase : PassiveBase, IHitEnhancementPassive
{
    protected HitEnhancementPassiveBase(IRandomManager randomManager)
        : base(randomManager)
    {
    }

    public abstract int DamageModifier(ICharacter aggressor, ICharacter victim, SchoolTypes damageType, int baseDamage);
}
