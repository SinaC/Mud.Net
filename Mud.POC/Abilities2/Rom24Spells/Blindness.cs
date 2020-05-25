using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class Blindness : CharacterDebuffSpellBase
    {
        public override int Id => 4;
        public override string Name => "Blindness";
        public override string CharacterWearOffMessage => "You can see again.";
        public override string DispelRoomMessage => "{0:N} is no longer blinded.";

        public Blindness(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet, auraManager)
        {
        }

        protected override SchoolTypes DebuffType => SchoolTypes.None;
        protected override string VictimAffectMessage => "You are blinded!";
        protected override string RoomAffectMessage => throw new NotImplementedException();

        protected override (int level, TimeSpan duration, IAffect[] affects) AuraInfo(ICharacter caster, int level, ICharacter victim)
            => (level, TimeSpan.FromHours(1 + level),
            new IAffect[]
            {
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = -4, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = CharacterFlags.Blind, Operator = AffectOperators.Add }
            });
    }
}
