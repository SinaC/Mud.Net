using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Detect Hidden", AbilityEffects.Buff | AbilityEffects.Detection)]
    [AbilityCharacterWearOffMessage("You feel less aware of your surroundings.")]
    [AbilityDispellable]
    public class DetectHidden : CharacterFlagsSpellBase
    {
        public DetectHidden(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet, auraManager)
        {
        }

        protected override CharacterFlags CharacterFlags => CharacterFlags.DetectHidden;
        protected override string SelfAlreadyAffected => "You are already as alert as you can be.";
        protected override string NotSelfAlreadyAffected => "{0:N} can already sense hidden lifeforms.";
        protected override string Success => "Your awareness improves.";
        protected override string NotSelfSuccess => "Ok.";
        protected override TimeSpan Duration => TimeSpan.FromMinutes(Level);
    }
}
