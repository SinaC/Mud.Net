using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Rom24Effects;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24BreathSpells
{
    [Spell(SpellName, AbilityEffects.DamageArea | AbilityEffects.Debuff)]
    public class GasBreath : NoTargetSpellBase
    {
        public const string SpellName = "Gas Breath";

        private IAuraManager AuraManager { get; }

        public GasBreath(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            Caster.Act(ActOptions.ToRoom, "{0} breathes out a cloud of poisonous gas!", Caster);
            Caster.Send("You breath out a cloud of poisonous gas.");

            int hp = Math.Max(16, Caster.HitPoints);
            int hpDamage = RandomManager.Range(1 + hp / 15, hp / 8);
            int diceDamage = RandomManager.Dice(Level, 12);
            int damage = Math.Max(hpDamage + diceDamage / 10, diceDamage + hpDamage / 10);

            new PoisonEffect(RandomManager, AuraManager).Apply(Caster.Room, Caster, SpellName, Level, damage);
            IReadOnlyCollection<ICharacter> clone = new ReadOnlyCollection<ICharacter>(Caster.Room.People.Where(x =>
                !(x.IsSafeSpell(Caster, true) 
                  || (x is INonPlayableCharacter && Caster is INonPlayableCharacter && (Caster.Fighting == x || x.Fighting == Caster)))).ToList());
            foreach (ICharacter victim in clone)
            {
                if (victim.SavesSpell(Level, SchoolTypes.Poison))
                {
                    new PoisonEffect(RandomManager, AuraManager).Apply(victim, Caster, SpellName, Level/2, damage/4);
                    victim.AbilityDamage(Caster, damage / 2, SchoolTypes.Poison, "blast of gas", true);
                }
                else
                {
                    new PoisonEffect(RandomManager, AuraManager).Apply(victim, Caster, SpellName, Level, damage);
                    victim.AbilityDamage(Caster, damage, SchoolTypes.Poison, "blast of gas", true);
                }
            }
        }
    }
}
