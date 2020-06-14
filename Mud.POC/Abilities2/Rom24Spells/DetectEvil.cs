using Mud.POC.Abilities2.Domain;
using Mud.Server.Random;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Buff | AbilityEffects.Detection)]
    [AbilityCharacterWearOffMessage("The red in your vision disappears.")]
    [AbilityDispellable]
    public class DetectEvil : CharacterFlagsSpellBase
    {
        public const string SpellName = "Detect Evil";

        public DetectEvil(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager, auraManager)
        {
        }

        protected override CharacterFlags CharacterFlags => CharacterFlags.DetectEvil;
        protected override string SelfAlreadyAffected => "You can already sense evil.";
        protected override string NotSelfAlreadyAffected => "{0:N} can already detect evil.";
        protected override string SelfSuccess => "Your eyes tingle.";
        protected override string NotSelfSuccess => "Ok.";
        protected override TimeSpan Duration => TimeSpan.FromMinutes(Level);
    }
}
