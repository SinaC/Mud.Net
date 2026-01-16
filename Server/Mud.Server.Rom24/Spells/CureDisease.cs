using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Cure), NotInCombat(Message = StringHelpers.YouLostYourConcentration)]
[Syntax("cast [spell] <character>")]
[Help(
@"This spell heals the plague.")]
[OneLineHelp("heals the plague")]
public class CureDisease : CureSpellBase
{
    private const string SpellName = "Cure Disease";

    public CureDisease(ILogger<CureDisease> logger, IRandomManager randomManager, IAbilityManager abilityManager, IDispelManager dispelManager)
        : base(logger, randomManager, abilityManager, dispelManager)
    {
    }

    protected override string ToCureAbilityName => "Plague";
    protected override string SelfNotFoundMsg => "You aren't ill.";
    protected override string NotSelfFoundMsg => "{0:N} doesn't appear to be diseased.";
}
