using Mud.Server.Common;
using Mud.POC.Abilities2.Rom24Effects;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Buff, PulseWaitTime = 24)]
    [AbilityCharacterWearOffMessage("Your rage ebbs.")]
    [AbilityDispellable("{0:N} no longer looks so wild.")]
    public class Frenzy : DefensiveSpellBase
    {
        public const string SpellName = "Frenzy";

        private IAuraManager AuraManager { get; }

        public Frenzy(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            FrenzyEffect effect = new FrenzyEffect(AuraManager);
            effect.Apply(Victim, Caster, SpellName, Level, 0);
        }
    }
}
