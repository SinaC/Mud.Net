﻿using System.Collections.Generic;
using Mud.Domain;
using Mud.Logger;
using SimpleInjector.Advanced;

namespace Mud.Server.Races
{
    public class Insectoid : RaceBase // 4-arms
    {
        #region IRace

        private readonly List<EquipmentSlots> _slots = new List<EquipmentSlots>
        {
            Domain.EquipmentSlots.Light,
            Domain.EquipmentSlots.Head,
            Domain.EquipmentSlots.Amulet,
            Domain.EquipmentSlots.Shoulders,
            Domain.EquipmentSlots.Chest,
            Domain.EquipmentSlots.Cloak,
            Domain.EquipmentSlots.Waist,
            Domain.EquipmentSlots.Wrists,
            Domain.EquipmentSlots.Arms,
            Domain.EquipmentSlots.Hands,
            Domain.EquipmentSlots.Ring,
            Domain.EquipmentSlots.Ring,
            Domain.EquipmentSlots.Legs,
            Domain.EquipmentSlots.Feet,
            Domain.EquipmentSlots.Trinket,
            Domain.EquipmentSlots.Trinket,
            Domain.EquipmentSlots.MainHand,
            Domain.EquipmentSlots.OffHand,
            Domain.EquipmentSlots.MainHand,
            Domain.EquipmentSlots.OffHand,
        };

        public override string Name => "insectoid";
        public override string ShortName => "Ins";

        public override IEnumerable<EquipmentSlots> EquipmentSlots => _slots;

        public override IRVFlags Immunities => IRVFlags.None;
        public override IRVFlags Resistances => IRVFlags.Bash | IRVFlags.Slash | IRVFlags.Poison | IRVFlags.Disease | IRVFlags.Acid;
        public override IRVFlags Vulnerabilities => IRVFlags.Pierce | IRVFlags.Fire | IRVFlags.Cold;

        public override int GetStartAttribute(CharacterAttributes attribute)
        {
            switch (attribute)
            {
                case CharacterAttributes.Strength: return 16;
                case CharacterAttributes.Intelligence: return 16;
                case CharacterAttributes.Wisdom: return 16;
                case CharacterAttributes.Dexterity: return 16;
                case CharacterAttributes.Constitution: return 16;
                case CharacterAttributes.MaxHitPoints: return 100;
                case CharacterAttributes.SavingThrow: return 0;
                case CharacterAttributes.HitRoll: return 0;
                case CharacterAttributes.DamRoll: return 0;
                case CharacterAttributes.MaxMovePoints: return 100;
                case CharacterAttributes.ArmorBash: return 100;
                case CharacterAttributes.ArmorPierce: return 100;
                case CharacterAttributes.ArmorSlash: return 100;
                case CharacterAttributes.ArmorMagic: return 100;
                default:
                    Log.Default.WriteLine(LogLevels.Error, "Unexpected attribute {0} for Insectoid", attribute);
                    return 0;
            }
        }

        public override int GetMaxAttribute(CharacterAttributes attribute)
        {
            switch (attribute)
            {
                case CharacterAttributes.Strength: return 22;
                case CharacterAttributes.Intelligence: return 22;
                case CharacterAttributes.Wisdom: return 22;
                case CharacterAttributes.Dexterity: return 22;
                case CharacterAttributes.Constitution: return 22;
                case CharacterAttributes.MaxHitPoints: return 100;
                case CharacterAttributes.SavingThrow: return 0;
                case CharacterAttributes.HitRoll: return 0;
                case CharacterAttributes.DamRoll: return 0;
                case CharacterAttributes.MaxMovePoints: return 100;
                case CharacterAttributes.ArmorBash: return 100;
                case CharacterAttributes.ArmorPierce: return 100;
                case CharacterAttributes.ArmorSlash: return 100;
                case CharacterAttributes.ArmorMagic: return 100;
                default:
                    Log.Default.WriteLine(LogLevels.Error, "Unexpected attribute {0} for Insectoid", attribute);
                    return 0;
            }
        }

        #endregion

        public Insectoid()
        {
            // TODO
            //AddAbility(1, AbilityManager.DualWieldAbility);
            //AddAbility(1, AbilityManager.ThirdWieldAbility);
            //AddAbility(1, AbilityManager.FourthWieldAbility);

            // Test race with all spells
            foreach (IAbility ability in AbilityManager.Spells)
                AddAbility(1, ability.Name, ResourceKinds.Mana, 5, CostAmountOperators.Percentage, 1);
        }
    }
}
