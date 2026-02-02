using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Ability.Spell;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;

namespace Mud.Server.Rom24.MegaSpells;

[Spell(SpellName, AbilityEffects.Damage)]
public class HighExplosive : DamageSpellBase
{
    private const string SpellName = "High Explosive";

    public HighExplosive(ILogger<HighExplosive> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override SchoolTypes DamageType => SchoolTypes.Pierce;
    protected override int DamageValue => RandomManager.Range(30, 120);
    protected override string DamageNoun => "high explosive ammo";
}
