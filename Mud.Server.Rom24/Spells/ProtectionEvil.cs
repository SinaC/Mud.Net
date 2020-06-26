using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Flags;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Random;
using System;

namespace Mud.Server.Rom24.Spells
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
                new CharacterFlagsAffect { Modifier = new CharacterFlags("ProtectEvil"), Operator = AffectOperators.Or }
            });
        
    }
}
