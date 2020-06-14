using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Damage)]
    public class EnergyDrain : OffensiveSpellBase
    {
        public const string SpellName = "Energy Drain";

        public EnergyDrain(IRandomManager randomManager)
            : base(randomManager)
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

            Victim.AbilityDamage(Caster, damage, SchoolTypes.Negative, "energy drain", true);
        }
    }
}
