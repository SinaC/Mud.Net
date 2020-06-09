using Mud.POC.Abilities2.Rom24Effects;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Healing)]
    public class Heal : DefensiveSpellBase
    {
        public const string SpellName = "Heal";

        public Heal(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override void Invoke()
        {
            HealEffect effect = new HealEffect();
            effect.Apply(Victim, Caster, SpellName, Level, 0);
        }
    }
}
