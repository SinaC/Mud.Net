using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;

namespace Mud.Server.Rom24.Spells
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
