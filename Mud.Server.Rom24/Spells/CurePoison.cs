using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Cure)]
public class CurePoison : CureSpellBase
{
    private const string SpellName = "Cure Poison";

    public CurePoison(ILogger<CurePoison> logger, IRandomManager randomManager, IAbilityManager abilityManager, IDispelManager dispelManager)
        : base(logger, randomManager, abilityManager, dispelManager)
    {
    }

    protected override string ToCureAbilityName => "Poison";
    protected override string SelfNotFoundMsg => "You aren't poisoned.";
    protected override string NotSelfFoundMsg => "{0:N} doesn't appear to be poisoned.";
}
