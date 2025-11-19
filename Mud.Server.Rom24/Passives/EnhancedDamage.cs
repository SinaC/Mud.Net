using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 6)]
public class EnhancedDamage : HitEnhancementPassiveBase
{
    private const string PassiveName = "Enhanced Damage";

    public EnhancedDamage(ILogger<EnhancedDamage> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    public override int DamageModifier(ICharacter aggressor, ICharacter victim, SchoolTypes damageType, int baseDamage)
    {
        bool isTriggered = IsTriggered(aggressor, victim, true, out var diceRoll, out _);
        if (isTriggered)
            return 2 * (baseDamage * diceRoll) / 300; // at most 66% more damage
        return 0;
    }
}
