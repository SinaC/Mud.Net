using Mud.POC.Abilities2.Domain;
using Mud.Server.Random;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Damage)]
    public class Harm : OffensiveSpellBase
    {
        public const string SpellName = "Harm";

        public Harm(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override void Invoke()
        {
            int damage = Math.Max(20, Victim.HitPoints - RandomManager.Dice(1, 4));
            if (Victim.SavesSpell(Level, SchoolTypes.Harm))
                damage = Math.Min(50, damage / 2);
            damage = Math.Min(100, damage);
            Victim.AbilityDamage(Caster, damage, SchoolTypes.Harm, "harm spell", true);
        }
    }
}
