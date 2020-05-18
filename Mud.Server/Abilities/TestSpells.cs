using System;
using Mud.Domain;
using Mud.Server.Aura;
using Mud.Server.Common;

namespace Mud.Server.Abilities
{
    public partial class AbilityManager
    {
        [Spell(999999, "Test", AbilityTargets.CharacterSelf, CharacterWearOffMessage = "Your divine aura wears off.", PulseWaitTime = 10 * Pulse.PulsePerSeconds, Cooldown = 10 * Pulse.PulsePerMinutes)]
        public void SpellTest(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.GetAura(ability) != null)
            {
                caster.Act( ActOptions.ToCharacter, "{0:N} {0:b} already affected by divine aura.", victim);
                return;
            }

            // Immune to all damages
            World.AddAura(victim,ability, caster, caster.Level, TimeSpan.FromMinutes(1), AuraFlags.NoDispel, true,
                new CharacterIRVAffect { Location = IRVAffectLocations.Immunities, Modifier = IRVFlags.Magic, Operator = AffectOperators.Or},
                new CharacterIRVAffect { Location = IRVAffectLocations.Immunities, Modifier = IRVFlags.Weapon, Operator = AffectOperators.Or });
        }
    }
}
