using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Infravision", AbilityEffects.Buff | AbilityEffects.Detection)]
    [AbilityCharacterWearOffMessage("You no longer see in the dark.")]
    [AbilityDispellable]
    public class Infravision : CharacterFlagsSpellBase
    {
        public Infravision(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet, auraManager)
        {
        }

        protected override CharacterFlags CharacterFlags => CharacterFlags.Infrared;
        protected override TimeSpan Duration => TimeSpan.FromMinutes(2*Level);
        protected override string SelfAlreadyAffected => "You can already see in the dark.";
        protected override string NotSelfAlreadyAffected => "{0} already has infravision.";
        protected override string SelfSuccess => "Your eyes glow red.";
        protected override string NotSelfSuccess => "{0:P} eyes glow red.";
    }
}
