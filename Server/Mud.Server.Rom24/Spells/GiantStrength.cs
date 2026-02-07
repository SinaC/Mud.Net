using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Flags;
using Mud.Random;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Character;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.SpellGuards;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Buff)]
[AbilityCharacterWearOffMessage("You feel weaker.")]
[AbilityDispellable("{0:N} no longer looks so mighty.")]
[Syntax("cast [spell] <character>")]
[Help(
@"This spell increases the strength of the target character.")]
[OneLineHelp("grants increased strength")]
public class GiantStrength : DefensiveSpellBase
{
    private const string SpellName = "Giant Strength";

    protected override ISpellGuard[] Guards => [new CannotBeInCombat()];

    private IAuraManager AuraManager { get; }

    public GiantStrength(ILogger<GiantStrength> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        AuraManager = auraManager;
    }

    protected override void Invoke()
    {
        if (Victim.GetAura(SpellName) != null)
        {
            if (Victim == Caster)
                Caster.Send("You are already as strong as you can get!");
            else
                Caster.Act(ActOptions.ToCharacter, "{0:N} can't get any stronger.", Victim);
            return;
        }
        int modifier = 1 + (Level >= 18 ? 1 : 0) + (Level >= 25 ? 1 : 0) + (Level >= 32 ? 1 : 0);
        AuraManager.AddAura(Victim, AbilityDefinition.Name, Caster, Level, TimeSpan.FromMinutes(Level), true,
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = modifier, Operator = AffectOperators.Add });
        Victim.Act(ActOptions.ToAll, "{0:P} muscles surge with heightened power.", Victim);
    }
}
