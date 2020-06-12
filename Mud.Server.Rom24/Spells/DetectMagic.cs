using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Random;
using System;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Buff | AbilityEffects.Detection)]
    [AbilityCharacterWearOffMessage("The detect magic wears off.")]
    [AbilityDispellable]
    public class DetectMagic : CharacterFlagsSpellBase
    {
        public const string SpellName = "Detect Magic";

        public DetectMagic(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager, auraManager)
        {
        }

        protected override CharacterFlags CharacterFlags => CharacterFlags.DetectInvis;
        protected override string SelfAlreadyAffected => "You can already sense magical auras.";
        protected override string NotSelfAlreadyAffected => "{0:N} can already detect magic.";
        protected override string SelfSuccess => "Your eyes tingle.";
        protected override string NotSelfSuccess => "Ok.";
        protected override TimeSpan Duration => TimeSpan.FromMinutes(Level);
    }
}
