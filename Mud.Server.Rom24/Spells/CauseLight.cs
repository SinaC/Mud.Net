using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Damage)]
public class CauseLight : DamageSpellBase
{
    private const string SpellName = "Cause Light";

    public CauseLight(ILogger<CauseLight> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override SchoolTypes DamageType => SchoolTypes.Harm;
    protected override int DamageValue => RandomManager.Dice(1, 8) + Level / 3;
    protected override string DamageNoun => "spell";
}
