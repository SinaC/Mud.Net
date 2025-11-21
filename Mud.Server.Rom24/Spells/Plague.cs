using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Common;
using Mud.Server.Flags;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using Mud.Server.Rom24.Affects;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Debuff | AbilityEffects.Damage)]
[AbilityCharacterWearOffMessage("Your sores vanish.")]
[Syntax("cast [spell] <target>")]
[Help(
@"The plague spell infests the target with a magical disease of great virulence,
sapping its strength and causing horrific suffering, possibly leading to
death.  It is a risky spell to use, as the contagion can spread like
wildfire if the victim makes it to a populated area.")]
public class Plague : OffensiveSpellBase
{
    private const string SpellName = "Plague";

    private IServiceProvider ServiceProvider { get; }
    private IAuraManager AuraManager { get; }

    public Plague(ILogger<Plague> logger, IServiceProvider serviceProvider, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        ServiceProvider = serviceProvider;
        AuraManager = auraManager;
    }

    protected override void Invoke()
    {
        if (Victim.SavesSpell(Level, SchoolTypes.Disease)
            || (Victim is INonPlayableCharacter npcVictim && npcVictim.ActFlags.IsSet("Undead")))
        {
            if (Victim == Caster)
                Caster.Send("You feel momentarily ill, but it passes.");
            else
                Caster.Act(ActOptions.ToCharacter, "{0:N} seems to be unaffected.", Victim);
            return;
        }

        AuraManager.AddAura(Victim, SpellName, Caster, (3 * Level) / 4, TimeSpan.FromMinutes(Level), AuraFlags.None, true,
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -5, Operator = AffectOperators.Add },
            new CharacterFlagsAffect { Modifier = new CharacterFlags(ServiceProvider, "Plague"), Operator = AffectOperators.Or },
            new PlagueSpreadAndDamageAffect(ServiceProvider, RandomManager, AuraManager));
        Victim.Act(ActOptions.ToAll, "{0:N} scream{0:V} in agony as plague sores erupt from {0:s} skin.", Victim);
    }
}
