using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Random;

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
