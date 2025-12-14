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
[OneLineHelp("causes the target to suffer a slow, painful death from plague")]
public class Plague : OffensiveSpellBase
{
    private const string SpellName = "Plague";

    private IAuraManager AuraManager { get; }

    public Plague(ILogger<Plague> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
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

        var level = (3 * Level) / 4;
        var duration = Level;
        var plagueAura = Victim.GetAura(SpellName);
        if (plagueAura != null)
            plagueAura.Update(level, TimeSpan.FromMinutes(duration));
        else
        {
            AuraManager.AddAura(Victim, SpellName, Caster, level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -5, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = new CharacterFlags("Plague"), Operator = AffectOperators.Or },
                new PlagueSpreadAndDamageAffect(RandomManager, AuraManager));
            Victim.Act(ActOptions.ToAll, "{0:N} scream{0:V} in agony as plague sores erupt from {0:s} skin.", Victim);
        }
    }
}
