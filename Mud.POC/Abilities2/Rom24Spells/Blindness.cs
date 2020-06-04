using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Blindness", AbilityEffects.Debuff)]
    [AbilityCharacterWearOffMessage("You can see again.")]
    [AbilityDispellable("{0:N} is no longer blinded.")]
    public class Blindness : CharacterDebuffSpellBase
    {
       public Blindness(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet, auraManager)
        {
        }

        protected override SchoolTypes DebuffType => SchoolTypes.None;
        protected override string VictimAffectMessage => "You are blinded!";
        protected override string RoomAffectMessage => "{0:N} is no longer blinded.";

        protected override (int level, TimeSpan duration, IAffect[] affects) AuraInfo
            => (Level, TimeSpan.FromHours(1 + Level),
            new IAffect[]
            {
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = -4, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = CharacterFlags.Blind, Operator = AffectOperators.Add }
            });
    }
}
