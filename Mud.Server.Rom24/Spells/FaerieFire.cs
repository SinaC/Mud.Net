using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Character;
using Mud.Server.Common;
using Mud.Server.Flags;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Debuff)]
[AbilityCharacterWearOffMessage("The pink aura around you fades away.")]
[AbilityDispellable("{0:N}'s outline fades.")]
[Syntax("cast [spell] <victim>")]
[Help(
@"This spell increases (makes worse) the armor class of its victim.  For each
level of the caster, the victim's armor class is increased by two points.")]
public class FaerieFire : OffensiveSpellBase
{
    private const string SpellName = "Faerie Fire";

    private IServiceProvider ServiceProvider { get; }
    private IAuraManager AuraManager { get; }

    public FaerieFire(ILogger<FaerieFire> logger, IServiceProvider serviceProvider, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        ServiceProvider = serviceProvider;
        AuraManager = auraManager;
    }

    protected override void Invoke()
    {
        if (Victim.CharacterFlags.IsSet("FaerieFire"))
            return;
        AuraManager.AddAura(Victim, SpellName, Caster, Level, TimeSpan.FromMinutes(Level), AuraFlags.None, true,
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = 2 * Level, Operator = AffectOperators.Add },
            new CharacterFlagsAffect { Modifier = new CharacterFlags(ServiceProvider, "FaerieFire"), Operator = AffectOperators.Or });
        Victim.Act(ActOptions.ToAll, "{0:N} are surrounded by a pink outline.", Victim);
    }
}
