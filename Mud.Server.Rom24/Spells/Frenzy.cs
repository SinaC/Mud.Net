using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;

namespace Mud.Server.Rom24.Spells
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
