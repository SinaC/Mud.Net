using System;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Rom24Effects;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24BreathSpells
{
    [Spell(SpellName, AbilityEffects.Damage | AbilityEffects.Debuff, PulseWaitTime = 24)]
    public class AcidBreath : OffensiveSpellBase
    {
        public const string SpellName = "Acid Breath";

        private IAuraManager AuraManager { get; }
        private IItemManager ItemManager { get; }

        public AcidBreath(IRandomManager randomManager, IAuraManager auraManager, IItemManager itemManager) : base(randomManager)
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
                AcidEffect effect = new AcidEffect(RandomManager, AuraManager, ItemManager);
                effect.Apply(Victim, Caster, SpellName, Level/2, damage/4);
                Victim.AbilityDamage(Caster, damage / 2, SchoolTypes.Acid, "blast of acid", true);
            }
            else
            {
                AcidEffect effect = new AcidEffect(RandomManager, AuraManager, ItemManager);
                effect.Apply(Victim, Caster, SpellName, Level, damage);
                Victim.AbilityDamage(Caster, damage, SchoolTypes.Acid, "blast of acid", true);
            }
        }
    }
}
