using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Domain.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;

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

    private IEffectManager EffectManager { get; }

    public Plague(ILogger<Plague> logger, IRandomManager randomManager, IEffectManager effectManager)
        : base(logger, randomManager)
    {
        EffectManager = effectManager;
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
            Victim.Act(ActOptions.ToAll, "{0:N} scream{0:V} in agony as plague sores erupt from {0:s} skin.", Victim);
            var plagueEffect = EffectManager.CreateInstance<ICharacter>("Plague");
            plagueEffect?.Apply(Victim, Caster, SpellName, level, -5);
        }
    }
}
