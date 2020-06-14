using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Random;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Buff)]
    [AbilityCharacterWearOffMessage("You feel less protected.")]
    [AbilityDispellable]
    public class ProtectionEvil : CharacterBuffSpellBase
    {
        public const string SpellName = "Protection Evil";

        public ProtectionEvil(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager, auraManager)
        {
        }

        protected override string SelfAlreadyAffectedMessage => "You are already protected.";
        protected override string NotSelfAlreadyAffectedMessage => "{0:N} {0:b} already protected.";
        protected override string VictimAffectMessage => "You feel holy and pure.";
        protected override string CasterAffectMessage => "{0:N} is protected from evil.";
        protected override (int level, TimeSpan duration, IAffect[] affects) AuraInfo
            => (Level, TimeSpan.FromMinutes(24),
            new IAffect[] 
            {
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.SavingThrow, Modifier = -1, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = CharacterFlags.ProtectEvil, Operator = AffectOperators.Or }
            });
        
    }
}
