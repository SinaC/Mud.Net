using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Detect Magic", AbilityEffects.Buff | AbilityEffects.Detection)]
    [AbilityCharacterWearOffMessage("The detect magic wears off.")]
    [AbilityDispellable]
    public class DetectMagic : CharacterFlagsSpellBase
    {
        public DetectMagic(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet, auraManager)
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
