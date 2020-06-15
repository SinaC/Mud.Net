using Mud.Domain;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Blueprints.Character;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.World;
using Mud.Server.Random;
using System;

namespace Mud.Server.Ability
{
    [Spell(SpellName, AbilityEffects.Buff, CooldownInSeconds = 10*60, PulseWaitTime = 10 * Pulse.PulsePerSeconds)]
    public class SpellTest : DefensiveSpellBase
    {
        public const string SpellName = "Test";

        private IAuraManager AuraManager { get; }

        public SpellTest(IRandomManager randomManager, IAuraManager auraManager) 
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            if (Victim.GetAura(SpellName) != null)
            {
                Caster.Act(ActOptions.ToCharacter, "{0:N} {0:b} already affected by divine aura.", Victim);
                return;
            }

            // Immune to all damages
            AuraManager.AddAura(Victim, SpellName, Caster, Level, TimeSpan.FromMinutes(1), AuraFlags.NoDispel, true,
                new CharacterIRVAffect { Location = IRVAffectLocations.Immunities, Modifier = IRVFlags.Magic, Operator = AffectOperators.Or },
                new CharacterIRVAffect { Location = IRVAffectLocations.Immunities, Modifier = IRVFlags.Weapon, Operator = AffectOperators.Or });
        }
    }

    [Spell(SpellName, AbilityEffects.Creation | AbilityEffects.Animation)]
    public class SpellConstruct : NoTargetSpellBase
    {
        public const string SpellName = "Construct";

        private IWorld World { get; }
        private IAuraManager AuraManager { get; }

        public SpellConstruct(IRandomManager randomManager, IWorld world, IAuraManager auraManager)
            : base(randomManager)
        {
            World = world;
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            if (Caster is IPlayableCharacter pcCaster)
            {
                CharacterBlueprintBase blueprint = World.GetCharacterBlueprint(80000);
                INonPlayableCharacter construct = World.AddNonPlayableCharacter(Guid.NewGuid(), blueprint, Caster.Room);
                pcCaster.AddPet(construct);
                AuraManager.AddAura(construct, SpellName, Caster, Level, Pulse.Infinite, AuraFlags.Permanent | AuraFlags.NoDispel, true,
                    new CharacterFlagsAffect { Modifier = CharacterFlags.Charm, Operator = AffectOperators.Or });
            }
        }
    }


    //    //[Spell(999998, "Construct", AbilityTargets.None)]
    //    //public void SpellConstruct(IAbility ability, int level, ICharacter caster)
    //    //{
    //    //}
    //}
}
