using Microsoft.Extensions.Logging;
using Mud.Random;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.SpellGuards;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Cure)]
[Syntax("cast [spell] <character>")]
[Help(
@"This spell heals the plague.")]
[OneLineHelp("heals the plague")]
public class CureDisease : CureSpellBase
{
    private const string SpellName = "Cure Disease";

    protected override ISpellGuard[] Guards => [new CannotBeInCombat()];

    public CureDisease(ILogger<CureDisease> logger, IRandomManager randomManager, IAbilityManager abilityManager, IDispelManager dispelManager)
        : base(logger, randomManager, abilityManager, dispelManager)
    {
    }

    protected override string ToCureAbilityName => "Plague";
    protected override string SelfNotFoundMsg => "You aren't ill.";
    protected override string NotSelfFoundMsg => "{0:N} doesn't appear to be diseased.";
}
