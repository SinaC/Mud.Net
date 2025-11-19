using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Damage)]
public class AcidBlast : DamageSpellBase
{
    private const string SpellName = "Acid Blast";

    public AcidBlast(ILogger<AcidBlast> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override SchoolTypes DamageType => SchoolTypes.Acid;
    protected override int DamageValue => RandomManager.Dice(Level, 12);
    protected override string DamageNoun => "acid blast";
}
