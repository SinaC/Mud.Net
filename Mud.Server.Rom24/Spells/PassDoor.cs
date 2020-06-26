using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Random;
using System;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Buff)]
    [AbilityCharacterWearOffMessage("You feel solid again.")]
    [AbilityDispellable]
    public class PassDoor : CharacterFlagsSpellBase
    {
        public const string SpellName = "Pass Door";

        public PassDoor(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager, auraManager)
        {
        }

        protected override ICharacterFlags CharacterFlags => new CharacterFlags("PassDoor");
        protected override TimeSpan Duration => TimeSpan.FromMinutes(RandomManager.Fuzzy(Level / 4));
        protected override string SelfAlreadyAffected => "You are already out of phase.";
        protected override string NotSelfAlreadyAffected => "{0:N} is already shifted out of phase.";
        protected override string SelfSuccess => "You turn translucent.";
        protected override string NotSelfSuccess => "{0} turns translucent.";
    }
}
