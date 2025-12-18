using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Cure)]
[Syntax("cast [spell] <character>")]
[Help(
@"This spell cures poison in one so unfortunate.")]
[OneLineHelp("removes the harmful effects of poison")]
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
