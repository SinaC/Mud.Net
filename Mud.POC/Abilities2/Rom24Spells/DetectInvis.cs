using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Detect Invis", AbilityEffects.Buff | AbilityEffects.Detection)]
    [AbilityCharacterWearOffMessage("You no longer see invisible objects.")]
    [AbilityDispellable]
    public class DetectInvis : CharacterFlagsSpellBase
    {
        public DetectInvis(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet, auraManager)
        {
        }

        protected override CharacterFlags CharacterFlags => CharacterFlags.DetectInvis;
        protected override string SelfAlreadyAffected => "You can already see invisible.";
        protected override string NotSelfAlreadyAffected => "{0:N} can already see invisible things.";
        protected override string SelfSuccess => "Your eyes tingle.";
        protected override string NotSelfSuccess => "Ok.";
        protected override TimeSpan Duration => TimeSpan.FromMinutes(Level);
    }
}
