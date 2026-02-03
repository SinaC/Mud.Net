using Microsoft.Extensions.Logging;
using Mud.Random;
using Mud.Server.Ability.Interfaces;
using Mud.Server.Ability.Spell;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Aura;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Cure)]
[Syntax("cast [spell] <character>")]
[Help(
@"This spell cures blindness in one so unfortunate.")]
[OneLineHelp("restored sight to the blind")]
public class CureBlindness : CureSpellBase
{
    private const string SpellName = "Cure Blindness";

    public CureBlindness(ILogger<CureBlindness> logger, IRandomManager randomManager, IAbilityManager abilityManager, IDispelManager dispelManager)
        : base(logger, randomManager, abilityManager, dispelManager)
    {
    }

    protected override string ToCureAbilityName => "Blindness";
    protected override string SelfNotFoundMsg => "You aren't blind.";
    protected override string NotSelfFoundMsg => "{0:N} doesn't appear to be blinded.";
}
