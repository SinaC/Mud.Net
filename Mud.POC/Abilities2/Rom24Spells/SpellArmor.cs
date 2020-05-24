using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class SpellArmor : CharacterBuffSpellBase
    {
        public override int Id => 2;
        public override string Name => "Armor";
        public override string CharacterWearOffMessage => "You feel less armored.";

        public SpellArmor(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager) 
            : base(randomManager, wiznet, auraManager)
        {
        }

        protected override string AlreadyAffectedMessage => "{0:N} is already armored.";
        protected override string VictimAffectMessage => "You feel someone protecting you.";
        protected override string CasterAffectMessage => "{0} is protected by your magic.";

        protected override (int level, TimeSpan duration, IAffect[] affects) AuraInfo(ICharacter caster, int level, ICharacter victim)
            => (level, TimeSpan.FromHours(24),
             new[]
             { 
                 new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = -20, Operator = AffectOperators.Add }
             });
    }
}
