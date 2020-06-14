using System;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;

namespace Mud.POC.Abilities2.Rom24Effects
{
    public class BlindnessEffect : IEffect<ICharacter>
    {
        private IAuraManager AuraManager { get; }

        public BlindnessEffect(IAuraManager auraManager)
        {
            AuraManager = auraManager;
        }

        public void Apply(ICharacter victim, IEntity source, string abilityName, int level, int _)
        {
            if (victim.CharacterFlags.HasFlag(CharacterFlags.Blind) || victim.GetAura(abilityName) != null || victim.SavesSpell(level, SchoolTypes.None))
                return;
            AuraManager.AddAura(victim, abilityName, source, level, TimeSpan.FromHours(1 + level), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = -4, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = CharacterFlags.Blind, Operator = AffectOperators.Add });
            victim.Act(ActOptions.ToCharacter, "You are blinded!", source);
            victim.Act(ActOptions.ToRoom, "{0:N} is no longer blinded.", victim);
        }
    }
}
