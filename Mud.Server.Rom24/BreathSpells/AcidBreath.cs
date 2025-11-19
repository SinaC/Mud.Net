using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;

namespace Mud.Server.Rom24.BreathSpells;

[Spell(SpellName, AbilityEffects.Damage | AbilityEffects.Debuff, PulseWaitTime = 24)]
public class AcidBreath : OffensiveSpellBase
{
    private const string SpellName = "Acid Breath";

    private IAuraManager AuraManager { get; }
    private IItemManager ItemManager { get; }

    public AcidBreath(ILogger<AcidBreath> logger, IRandomManager randomManager, IAuraManager auraManager, IItemManager itemManager)
        : base(logger, randomManager)
    {
        AuraManager = auraManager;
        ItemManager = itemManager;
    }

    protected override void Invoke()
    {
        Caster.ActToNotVictim(Victim, "{0} spits acid at {1}.", Caster, Victim);
        Victim.Act(ActOptions.ToCharacter, "{0} spits a stream of corrosive acid at you.", Caster);
        Caster.Act(ActOptions.ToCharacter, "You spit acid at {0}.", Victim);

        int hp = Math.Max(12, Victim.HitPoints);
        int hpDamage = RandomManager.Range(1 + hp / 11, hp / 6);
        int diceDamage = RandomManager.Dice(Level, 16);
        int damage = Math.Max(hpDamage + diceDamage / 10, diceDamage + hpDamage / 10);

        if (Victim.SavesSpell(Level, SchoolTypes.Acid))
        {
            AcidEffect effect = new (Logger, RandomManager, AuraManager, ItemManager);
            effect.Apply(Victim, Caster, SpellName, Level/2, damage/4);
            Victim.AbilityDamage(Caster, damage / 2, SchoolTypes.Acid, "blast of acid", true);
        }
        else
        {
            AcidEffect effect = new (Logger, RandomManager, AuraManager, ItemManager);
            effect.Apply(Victim, Caster, SpellName, Level, damage);
            Victim.AbilityDamage(Caster, damage, SchoolTypes.Acid, "blast of acid", true);
        }
    }
}
