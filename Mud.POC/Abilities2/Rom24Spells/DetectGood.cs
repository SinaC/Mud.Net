using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Detect Good", AbilityEffects.Buff | AbilityEffects.Detection)]
    [AbilityCharacterWearOffMessage("The gold in your vision disappears.")]
    [AbilityDispellable]
    public class DetectGood : CharacterFlagsSpellBase
    {
        public DetectGood(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet, auraManager)
        {
        }

        protected override CharacterFlags CharacterFlags => CharacterFlags.DetectGood;
        protected override string SelfAlreadyAffected => "You can already sense good.";
        protected override string NotSelfAlreadyAffected => "{0:N} can already detect good.";
        protected override string Success => "Your eyes tingle.";
        protected override string NotSelfSuccess => "Ok.";
        protected override TimeSpan Duration => TimeSpan.FromMinutes(Level);
    }
}
