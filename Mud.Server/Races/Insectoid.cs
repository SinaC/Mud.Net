using System.Collections.Generic;
using Mud.Domain;

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

        public override int GetAttributeModifier(CharacterAttributes attribute)
        {
            return 0; // TODO: http://wow.gamepedia.com/Base_attributes#Racial_modifiers
        }

        #endregion

        public Insectoid()
        {
            AddAbility(1, AbilityManager.DualWieldAbility);
            AddAbility(1, AbilityManager.ThirdWieldAbility);
            AddAbility(1, AbilityManager.FourthWieldAbility);
        }
    }
}
