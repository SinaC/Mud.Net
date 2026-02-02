using Microsoft.Extensions.Logging;
using Mud.Random;
using Mud.Server.Ability.Interfaces;
using Mud.Server.Ability.Spell;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.SpellGuards;
using Mud.Server.Interfaces.Aura;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Cure)]
[Syntax("cast [spell] <character>")]
[Help(
@"This spell cures poison in one so unfortunate.")]
[OneLineHelp("removes the harmful effects of poison")]
public class CurePoison : CureSpellBase
{
    private const string SpellName = "Cure Poison";

    protected override ISpellGuard[] Guards => [new CannotBeInCombat()];

    public CurePoison(ILogger<CurePoison> logger, IRandomManager randomManager, IAbilityManager abilityManager, IDispelManager dispelManager)
        : base(logger, randomManager, abilityManager, dispelManager)
    {
    }

    protected override string ToCureAbilityName => "Poison";
    protected override string SelfNotFoundMsg => "You aren't poisoned.";
    protected override string NotSelfFoundMsg => "{0:N} doesn't appear to be poisoned.";
}
