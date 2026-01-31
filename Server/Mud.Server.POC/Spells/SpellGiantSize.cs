using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Character;
using Mud.Server.Domain;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;

namespace Mud.Server.POC.Spells;

[Spell(SpellName, AbilityEffects.Buff)]
public class SpellGiantSize : CharacterBuffSpellBase
{
    private const string SpellName = "Giant Size";

    public SpellGiantSize(ILogger<SpellGiantSize> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager, auraManager)
    {
    }

    protected override string SelfAlreadyAffectedMessage => "You are already affected.";
    protected override string NotSelfAlreadyAffectedMessage => "{0:N} is already affected.";
    protected override string VictimAffectMessage => "You are now affected by Giant Size.";
    protected override string CasterAffectMessage => "{0:N} {0:b} now affected by Giant Size.";

    protected override (int level, TimeSpan duration, IAffect[] affects) AuraInfo => (Caster.Level, TimeSpan.FromMinutes(5),
        new IAffect[]
        {
            new CharacterSizeAffect { Value = Sizes.Giant },
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = 10, Operator = AffectOperators.Add },
            new CharacterResourceAffect { Location = ResourceKinds.HitPoints, Modifier = Caster.Level * 8, Operator = AffectOperators.Add },
        });
}
