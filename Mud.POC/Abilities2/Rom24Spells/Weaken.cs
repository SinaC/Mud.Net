using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Weaken", AbilityEffects.Debuff)]
    [AbilityCharacterWearOffMessage("You feel stronger.")]
    [AbilityDispellable("{0:N} looks stronger.")]
    public class Weaken : CharacterDebuffSpellBase
    {
        public Weaken(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet, auraManager)
        {
        }

        protected override SchoolTypes DebuffType => SchoolTypes.Other;
        protected override string VictimAffectMessage => "You feel your strength slip away.";
        protected override string RoomAffectMessage => "{0:N} looks tired and weak.";
        protected override (int level, TimeSpan duration, IAffect[] affects) AuraInfo
            => (Level, TimeSpan.FromMinutes(Level / 2),
            new IAffect[]
            {
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -Level/5, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = CharacterFlags.Weaken, Operator = AffectOperators.Or }
            });

        protected override bool CanAffect() => base.CanAffect() && !Victim.CharacterFlags.HasFlag(CharacterFlags.Weaken);
    }
}
