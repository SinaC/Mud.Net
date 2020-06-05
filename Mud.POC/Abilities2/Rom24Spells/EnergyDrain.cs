using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Energy Drain", AbilityEffects.Damage)]
    public class EnergyDrain : OffensiveSpellBase
    {
        public EnergyDrain(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override void Invoke()
        {
            if (Victim != Caster)
                Caster.UpdateAlignment(-50);

            if (Victim.SavesSpell(Level, SchoolTypes.Negative))
            {
                Victim.Send("You feel a momentary chill.");
                return;
            }

            int damage;
            if (Victim.Level <= 2)
                damage = Victim.HitPoints + 1;
            else
            {
                damage = RandomManager.Dice(1, Level);
                if (Victim is IPlayableCharacter pcVictim)
                {
                    int lose = RandomManager.Range(Level / 2, 3 * Level / 2);
                    pcVictim.GainExperience(-lose);
                }
                Victim.UpdateResource(ResourceKinds.Mana, -Victim[ResourceKinds.Mana] / 2); // half mana
                Victim.UpdateMovePoints(-Victim.MovePoints / 2); // half move
                Caster.UpdateHitPoints(damage);
            }

            Victim.Send("You feel your life slipping away!");
            Caster.Send("Wow....what a rush!");

            Victim.AbilityDamage(Caster, this, damage, SchoolTypes.Negative, "energy drain", true);
        }
    }
}
