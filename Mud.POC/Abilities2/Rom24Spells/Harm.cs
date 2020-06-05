using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Harm", AbilityEffects.Damage)]
    public class Harm : OffensiveSpellBase
    {
        public Harm(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override void Invoke()
        {
            int damage = Math.Max(20, Victim.HitPoints - RandomManager.Dice(1, 4));
            if (Victim.SavesSpell(Level, SchoolTypes.Harm))
                damage = Math.Min(50, damage / 2);
            damage = Math.Min(100, damage);
            Victim.AbilityDamage(Caster, this, damage, SchoolTypes.Harm, "harm spell", true);
        }
    }
}
