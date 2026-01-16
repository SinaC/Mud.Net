using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.Character;
using Mud.Random;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 6)]
[Help(
@"Warriors and skilled thieves can become skilled enough in combat that they are
able to inflict more damage than other classes.  Enhanced damage is checked
for with each hit, although with a low skill, the chance of receiving a bonus
is very low indeed.")]
[OneLineHelp("this skill multiplies your damage in battle")]
public class EnhancedDamage : HitEnhancementPassiveBase
{
    private const string PassiveName = "Enhanced Damage";

    protected override string Name => PassiveName;

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
