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
            Domain.EquipmentSlots.RingLeft,
            Domain.EquipmentSlots.RingRight,
            Domain.EquipmentSlots.Legs,
            Domain.EquipmentSlots.Feet,
            Domain.EquipmentSlots.Trinket1,
            Domain.EquipmentSlots.Trinket2,
            Domain.EquipmentSlots.Wield,
            Domain.EquipmentSlots.Wield2,
            Domain.EquipmentSlots.Wield3,
            Domain.EquipmentSlots.Wield4,
        };

        public override string Name => "insectoid";
        public override string ShortName => "Ins";

        public override IEnumerable<EquipmentSlots> EquipmentSlots => _slots;

        public override int GetPrimaryAttributeModifier(PrimaryAttributeTypes primaryAttribute)
        {
            return 0; // TODO: http://wow.gamepedia.com/Base_attributes#Racial_modifiers
        }

        #endregion

        public Insectoid()
        {
            AddAbility(1, AbilityManager.ThirdWieldAbility);
        }
    }
}
