using Mud.Domain;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Blueprints.Character;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using System;
using Mud.Server.Ability;
using Mud.Server.Flags;
using Mud.Server.Interfaces.Affect;

namespace Mud.Server.Abilities
{
    [Spell(SpellName, AbilityEffects.Buff, CooldownInSeconds = 1)]
    public class SpellTest : ItemOrDefensiveSpellBase
    {
        public const string SpellName = "Test";

        private IAuraManager AuraManager { get; }

        public SpellTest(IRandomManager randomManager, IAuraManager auraManager) 
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke(ICharacter victim)
        {
            if (victim != Caster)
            {
                victim.ChangeStunned(5);
                return;
            }
            if (victim.GetAura(SpellName) != null)
            {
                Caster.Act(ActOptions.ToCharacter, "{0:N} {0:b} already affected by divine aura.", victim);
                return;
            }

            // Immune to all damages
            AuraManager.AddAura(victim, SpellName, Caster, Level, TimeSpan.FromMinutes(1), AuraFlags.NoDispel, true,
                new CharacterFlagsAffect { Modifier = new CharacterFlags("Pouet")},
                new CharacterIRVAffect { Location = IRVAffectLocations.Immunities, Modifier = new IRVFlags("Magic"), Operator = AffectOperators.Or },
                new CharacterIRVAffect { Location = IRVAffectLocations.Immunities, Modifier = new IRVFlags("Weapon"), Operator = AffectOperators.Or });
        }

        protected override void Invoke(IItem item)
        {
            if (item.GetAura(SpellName) != null)
            {
                Caster.Act(ActOptions.ToCharacter, "{0} is already affected by divine aura.", item);
                return;
            }

            if (item is IItemWeapon itemWeapon)
            {
                AuraManager.AddAura(itemWeapon, SpellName, Caster, Level, TimeSpan.FromMinutes(10), AuraFlags.NoDispel, true,
                    new ItemWeaponFlagsAffect { Modifier = new WeaponFlags("Flaming", "Frost", "Vampiric", "Sharp", "Vorpal", "Shocking", "Poison") });
            }

            AuraManager.AddAura(item, SpellName, Caster, Level, TimeSpan.FromMinutes(10), AuraFlags.NoDispel, true,
                new ItemFlagsAffect { Modifier = new ItemFlags("Glowing", "Humming", "Magic") },
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = -Level, Operator = AffectOperators.Add},
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Characteristics, Modifier = Level, Operator = AffectOperators.Add });
        }
    }

    [Spell(SpellName, AbilityEffects.Creation | AbilityEffects.Animation)]
    public class SpellConstruct : NoTargetSpellBase
    {
        public const string SpellName = "Construct";

        private ICharacterManager CharacterManager { get; }
        private IAuraManager AuraManager { get; }

        public SpellConstruct(IRandomManager randomManager, ICharacterManager characterManager, IAuraManager auraManager)
            : base(randomManager)
        {
            CharacterManager = characterManager;
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            if (Caster is IPlayableCharacter pcCaster)
            {
                CharacterBlueprintBase blueprint = CharacterManager.GetCharacterBlueprint(80000);
                INonPlayableCharacter construct = CharacterManager.AddNonPlayableCharacter(Guid.NewGuid(), blueprint, Caster.Room);
                pcCaster.AddPet(construct);
                AuraManager.AddAura(construct, SpellName, Caster, Level, Pulse.Infinite, AuraFlags.Permanent | AuraFlags.NoDispel, true,
                    new CharacterFlagsAffect { Modifier = new CharacterFlags("Charm"), Operator = AffectOperators.Or });
            }
        }
    }

    [Spell(SpellName, AbilityEffects.Buff)]
    public class SpellTestRoom : CharacterBuffSpellBase
    {
        private const string SpellName = "Testroom";

        public SpellTestRoom(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager, auraManager)
        {
        }

        protected override string SelfAlreadyAffectedMessage => "You are already affected.";
        protected override string NotSelfAlreadyAffectedMessage => "{0:N} is already affected.";
        protected override string VictimAffectMessage => "You are now affected by Testroom.";
        protected override string CasterAffectMessage => "{0:N} {0:b} now affected by Testroom.";

        protected override (int level, TimeSpan duration, IAffect[] affects) AuraInfo => (Caster.Level, TimeSpan.FromMinutes(5),
            new IAffect[]
            {
                new RoomHealRateAffect {Modifier = 10, Operator = AffectOperators.Add},
                new RoomResourceRateAffect {Modifier = 150, Operator = AffectOperators.Assign},
                new RoomFlagsAffect {Modifier = new RoomFlags("NoScan", "NoWhere"), Operator = AffectOperators.Or}
            });

        protected override void Invoke()
        {
            base.Invoke();

            Victim.Room.Recompute();
        }
    }

    //    //[Spell(999998, "Construct", AbilityTargets.None)]
    //    //public void SpellConstruct(IAbility ability, int level, ICharacter caster)
    //    //{
    //    //}
    //}
}
