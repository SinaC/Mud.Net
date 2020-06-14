using Mud.POC.Abilities2.Domain;
using Mud.Server.Random;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Buff)]
    [AbilityCharacterWearOffMessage("The white aura around your body fades.")]
    [AbilityDispellable("The white aura around {0:n}'s body vanishes.")]
    public class Sanctuary : CharacterFlagsSpellBase
    {
        public const string SpellName = "Sanctuary";

        public Sanctuary(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager, auraManager)
        {
        }

        protected override CharacterFlags CharacterFlags => CharacterFlags.Sanctuary;
        protected override TimeSpan Duration => TimeSpan.FromMinutes(Level / 6);
        protected override string SelfAlreadyAffected => "You are already in sanctuary.";
        protected override string NotSelfAlreadyAffected => "{0:N} is already in sanctuary.";
        protected override string SelfSuccess => "You are surrounded by a white aura.";
        protected override string NotSelfSuccess => "{0:N} is surrounded by a white aura.";
    }
}
