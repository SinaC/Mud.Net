using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Healing)]
public class CureLight : HealSpellBase
{
    private const string SpellName = "Cure Light";

    public CureLight(ILogger<CureLight> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override string HealVictimPhrase => "You feel better!";
    protected override int HealValue => RandomManager.Dice(1, 8) + Level / 3;
}
