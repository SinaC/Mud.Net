using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Rom24Effects;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.HealingArea, PulseWaitTime = 36)]
    public class MassHealing : NoTargetSpellBase
    {
        public const string SpellName = "Mass Healing";

        public MassHealing(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override void Invoke()
        {
            foreach (ICharacter victim in Caster.Room.People)
            {
                if ((Caster is IPlayableCharacter && victim is IPlayableCharacter)
                    || (Caster is INonPlayableCharacter && victim is INonPlayableCharacter))
                {
                    HealEffect healEffect = new HealEffect();
                    healEffect.Apply(victim, Caster, Heal.SpellName, Level, 0);
                    RefreshEffect refreshEffect = new RefreshEffect();
                    refreshEffect.Apply(victim, Caster, Refresh.SpellName, Level, 0);
                }
            }
        }
    }
}
