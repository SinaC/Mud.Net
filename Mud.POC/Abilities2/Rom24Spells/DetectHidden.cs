using Mud.POC.Abilities2.Domain;
using Mud.Server.Random;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Buff | AbilityEffects.Detection)]
    [AbilityCharacterWearOffMessage("You feel less aware of your surroundings.")]
    [AbilityDispellable]
    public class DetectHidden : CharacterFlagsSpellBase
    {
        public const string SpellName = "Detect Hidden";

        public DetectHidden(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager, auraManager)
        {
        }

        protected override CharacterFlags CharacterFlags => CharacterFlags.DetectHidden;
        protected override string SelfAlreadyAffected => "You are already as alert as you can be.";
        protected override string NotSelfAlreadyAffected => "{0:N} can already sense hidden lifeforms.";
        protected override string SelfSuccess => "Your awareness improves.";
        protected override string NotSelfSuccess => "Ok.";
        protected override TimeSpan Duration => TimeSpan.FromMinutes(Level);
    }
}
