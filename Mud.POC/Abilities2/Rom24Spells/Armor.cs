using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Random;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Buff)]
    [AbilityCharacterWearOffMessage("You feel less armored.")]
    public class Armor : CharacterBuffSpellBase
    {
        public const string SpellName = "Armor";

        public Armor(IRandomManager randomManager, IAuraManager auraManager) 
            : base(randomManager, auraManager)
        {
        }

        protected override string SelfAlreadyAffectedMessage => "You are already armored.";
        protected override string NotSelfAlreadyAffectedMessage => "{0:N} {0:b} already armored.";
        protected override string VictimAffectMessage => "You feel someone protecting you.";
        protected override string CasterAffectMessage => "{0:N} is protected by your magic.";
        protected override (int level, TimeSpan duration, IAffect[] affects) AuraInfo
            => (Level, TimeSpan.FromMinutes(24),
             new[]
             { 
                 new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = -20, Operator = AffectOperators.Add }
             });
    }
}
