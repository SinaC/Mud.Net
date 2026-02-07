using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Flags;
using Mud.Random;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Character;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Debuff)]
[AbilityCharacterWearOffMessage("The pink aura around you fades away.")]
[AbilityDispellable("{0:N}'s outline fades.")]
[Syntax("cast [spell] <victim>")]
[Help(
@"This spell increases (makes worse) the armor class of its victim.  For each
level of the caster, the victim's armor class is increased by two points.")]
[OneLineHelp("surrounds the target in a glowing aura")]
public class FaerieFire : OffensiveSpellBase
{
    private const string SpellName = "Faerie Fire";

    private IAuraManager AuraManager { get; }

    public FaerieFire(ILogger<FaerieFire> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        AuraManager = auraManager;
    }

    protected override void Invoke()
    {
        if (Victim.CharacterFlags.IsSet("FaerieFire"))
            return;
        AuraManager.AddAura(Victim, AbilityDefinition.Name, Caster, Level, TimeSpan.FromMinutes(Level), true,
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = 2 * Level, Operator = AffectOperators.Add },
            new CharacterFlagsAffect { Modifier = new CharacterFlags("FaerieFire"), Operator = AffectOperators.Or });
        Victim.Act(ActOptions.ToAll, "{0:N} are surrounded by a pink outline.", Victim);
    }
}
