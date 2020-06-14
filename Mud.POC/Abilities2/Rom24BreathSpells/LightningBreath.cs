using System;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Rom24Effects;
using Mud.Server.Random;

namespace Mud.POC.Abilities2.Rom24BreathSpells
{
    [Spell(SpellName, AbilityEffects.Damage | AbilityEffects.Debuff)]
    public class LightningBreath : OffensiveSpellBase
    {
        public const string SpellName = "Lightning Breath";

        private IAuraManager AuraManager { get; }
        private IItemManager ItemManager { get; }

        public LightningBreath(IRandomManager randomManager, IAuraManager auraManager, IItemManager itemManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
            ItemManager = itemManager;
        }

        protected override void Invoke()
        {
            Caster.ActToNotVictim(Victim, "{0} breathes a bolt of lightning at {1}.", Caster, Victim);
            Victim.Act(ActOptions.ToCharacter, "{0} breathes a bolt of lightning at you!", Caster);
            Caster.Act(ActOptions.ToCharacter, "You breathe a bolt of lightning at {0}.", Victim);

            int hp = Math.Max(10, Victim.HitPoints);
            int hpDamage = RandomManager.Range(1 + hp / 9, hp / 5);
            int diceDamage = RandomManager.Dice(Level, 20);
            int damage = Math.Max(hpDamage + diceDamage / 10, diceDamage + hpDamage / 10);

            if (Victim.SavesSpell(Level, SchoolTypes.Lightning))
            {
                new ShockEffect(RandomManager, AuraManager, ItemManager).Apply(Victim, Caster, SpellName, Level / 2, damage / 4);
                Victim.AbilityDamage(Caster, damage / 2, SchoolTypes.Lightning, "blast of lightning", true);
            }
            else
            {
                new ShockEffect(RandomManager, AuraManager, ItemManager).Apply(Victim, Caster, SpellName, Level, damage);
                Victim.AbilityDamage(Caster, damage, SchoolTypes.Lightning, "blast of lightning", true);
            }
        }
    }
}
