using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Detect Evil", AbilityEffects.Buff | AbilityEffects.Detection)]
    [AbilityCharacterWearOffMessage("The red in your vision disappears.")]
    [AbilityDispellable]
    public class DetectEvil : CharacterFlagsSpellBase
    {
        public DetectEvil(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet, auraManager)
        {
        }

        protected override CharacterFlags CharacterFlags => CharacterFlags.DetectEvil;
        protected override string SelfAlreadyAffected => "You can already sense evil.";
        protected override string NotSelfAlreadyAffected => "{0:N} can already detect evil.";
        protected override string Success => "Your eyes tingle.";
        protected override string NotSelfSuccess => "Ok.";
        protected override TimeSpan Duration => TimeSpan.FromMinutes(Level);
    }
}
