using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Pass Door", AbilityEffects.Buff)]
    [AbilityCharacterWearOffMessage("You feel solid again.")]
    [AbilityDispellable]
    public class PassDoor : CharacterFlagsSpellBase
    {
        public PassDoor(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet, auraManager)
        {
        }

        protected override CharacterFlags CharacterFlags => CharacterFlags.PassDoor;
        protected override TimeSpan Duration => TimeSpan.FromMinutes(RandomManager.Fuzzy(Level / 4));
        protected override string SelfAlreadyAffected => "You are already out of phase.";
        protected override string NotSelfAlreadyAffected => "{0:N} is already shifted out of phase.";
        protected override string SelfSuccess => "You turn translucent.";
        protected override string NotSelfSuccess => "{0} turns translucent.";
    }
}
