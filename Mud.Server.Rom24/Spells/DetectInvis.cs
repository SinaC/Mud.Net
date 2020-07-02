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
    [Spell(SpellName, AbilityEffects.Buff | AbilityEffects.Detection)]
    [AbilityCharacterWearOffMessage("You no longer see invisible objects.")]
    [AbilityDispellable]
    public class DetectInvis : CharacterFlagsSpellBase
    {
        public const string SpellName = "Detect Invis";

        public DetectInvis(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager, auraManager)
        {
        }

        protected override ICharacterFlags CharacterFlags => new CharacterFlags("DetectInvis");
        protected override string SelfAlreadyAffected => "You can already see invisible.";
        protected override string NotSelfAlreadyAffected => "{0:N} can already see invisible things.";
        protected override string SelfSuccess => "Your eyes tingle.";
        protected override string NotSelfSuccess => "Ok.";
        protected override TimeSpan Duration => TimeSpan.FromMinutes(Level);
    }
}
