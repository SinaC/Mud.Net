using Mud.POC.Abilities2.Rom24Effects;
using Mud.Server.Random;

namespace Mud.POC.Abilities2.Rom24Spells
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
