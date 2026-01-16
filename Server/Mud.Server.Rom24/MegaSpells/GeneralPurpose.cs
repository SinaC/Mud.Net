using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Domain;
using Mud.Server.Random;

namespace Mud.Server.Rom24.MegaSpells;

[Spell(SpellName, AbilityEffects.Damage)]
public class GeneralPurpose : DamageSpellBase
{
    private const string SpellName = "General Purpose";

    public GeneralPurpose(ILogger<GeneralPurpose> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override SchoolTypes DamageType => SchoolTypes.Pierce;
    protected override int DamageValue => RandomManager.Range(25, 100);
    protected override string DamageNoun => "general purpose ammo";
}
