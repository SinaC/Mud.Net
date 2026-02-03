using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Ability.Passive.Interfaces;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Ability.Passive;

public abstract class HitEnhancementPassiveBase : PassiveBase, IHitEnhancementPassive
{
    protected HitEnhancementPassiveBase(ILogger<HitEnhancementPassiveBase> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    public abstract int DamageModifier(ICharacter aggressor, ICharacter victim, SchoolTypes damageType, int baseDamage);
}
