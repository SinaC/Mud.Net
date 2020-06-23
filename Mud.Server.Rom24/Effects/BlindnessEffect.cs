using Mud.Domain;
using Mud.Server.Affects;
using Mud.Server.Effects;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Entity;
using System;

namespace Mud.Server.Rom24.Effects
{
    [Effect("Blindness")]
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
            AuraManager.AddAura(victim, abilityName, source, level, TimeSpan.FromMinutes(1 + level), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = -4, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = CharacterFlags.Blind, Operator = AffectOperators.Add });
            victim.Send("You are blinded!");
            victim.Act(ActOptions.ToRoom, "{0:N} appears to be blinded.", victim);
        }
    }
}
