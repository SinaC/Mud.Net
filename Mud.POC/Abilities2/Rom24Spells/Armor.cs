using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Armor", AbilityEffects.Buff)]
    [AbilityCharacterWearOffMessage("You feel less armored.")]
    public class Armor : CharacterBuffSpellBase
    {
        public Armor(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager) 
            : base(randomManager, wiznet, auraManager)
        {
        }

        protected override string AlreadyAffectedMessage => "{0:N} is already armored.";
        protected override string VictimAffectMessage => "You feel someone protecting you.";
        protected override string CasterAffectMessage => "{0} is protected by your magic.";

        protected override (int level, TimeSpan duration, IAffect[] affects) AuraInfo
            => (Level, TimeSpan.FromHours(24),
             new[]
             { 
                 new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = -20, Operator = AffectOperators.Add }
             });
    }
}
