using Mud.Server.Ability;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Healing, PulseWaitTime = 18)]
    public class Refresh : DefensiveSpellBase
    {
        public const string SpellName = "Refresh";

        public Refresh(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override void Invoke()
        {
            RefreshEffect effect = new RefreshEffect();
            effect.Apply(Victim, Caster, SpellName, Level, 0);
        }
    }
}
