using Mud.POC.Abilities2.Domain;
using Mud.Server.Random;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Buff | AbilityEffects.Detection)]
    [AbilityCharacterWearOffMessage("The gold in your vision disappears.")]
    [AbilityDispellable]
    public class DetectGood : CharacterFlagsSpellBase
    {
        public const string SpellName = "Detect Good";

        public DetectGood(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager, auraManager)
        {
        }

        protected override CharacterFlags CharacterFlags => CharacterFlags.DetectGood;
        protected override string SelfAlreadyAffected => "You can already sense good.";
        protected override string NotSelfAlreadyAffected => "{0:N} can already detect good.";
        protected override string SelfSuccess => "Your eyes tingle.";
        protected override string NotSelfSuccess => "Ok.";
        protected override TimeSpan Duration => TimeSpan.FromMinutes(Level);
    }
}
