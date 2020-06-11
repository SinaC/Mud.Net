﻿using System;
using Mud.Domain;
using Mud.Server.Affect;
using Mud.Server.Blueprints.Character;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Ability
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

        [Spell(999998, "Construct", AbilityTargets.None)]
        public void SpellConstruct(IAbility ability, int level, ICharacter caster)
        {
            if (caster is IPlayableCharacter pcCaster)
            {
                CharacterBlueprintBase blueprint = World.GetCharacterBlueprint(80000);
                INonPlayableCharacter construct = World.AddNonPlayableCharacter(Guid.NewGuid(), blueprint, caster.Room);
                pcCaster.AddPet(construct);
                World.AddAura(construct, ability, caster, level, Pulse.Infinite, AuraFlags.Permanent | AuraFlags.NoDispel, true,
                    new CharacterFlagsAffect { Modifier = CharacterFlags.Charm, Operator = AffectOperators.Or });
            }
        }
    }
}
