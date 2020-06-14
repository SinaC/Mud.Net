using Mud.Server.Random;
using Mud.POC.Abilities2.Rom24Effects;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Debuff)]
    [AbilityCharacterWearOffMessage("You can see again.")]
    [AbilityDispellable("{0:N} is no longer blinded.")]
    public class Blindness : DefensiveSpellBase
    {
        public const string SpellName = "Blindness";

        private IAuraManager AuraManager { get; }

        public Blindness(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            BlindnessEffect effect = new BlindnessEffect(AuraManager);
            effect.Apply(Victim, Caster, SpellName, Level, 0);
        }
    }
}
