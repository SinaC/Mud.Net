using System.Collections.Generic;
using Mud.Server.Constants;

namespace Mud.Server.Races
{
    public class Insectoid : RaceBase // 4-arms
    {
        #region IRace

        private readonly List<EquipmentSlots> _slots = new List<EquipmentSlots>
        {
            Constants.EquipmentSlots.Light,
                Constants.EquipmentSlots.Head,
                Constants.EquipmentSlots.Amulet,
                Constants.EquipmentSlots.Shoulders,
                Constants.EquipmentSlots.Chest,
                Constants.EquipmentSlots.Cloak,
                Constants.EquipmentSlots.Waist,
                Constants.EquipmentSlots.Wrists,
                Constants.EquipmentSlots.Arms,
                Constants.EquipmentSlots.Hands,
                Constants.EquipmentSlots.RingLeft,
                Constants.EquipmentSlots.RingRight,
                Constants.EquipmentSlots.Legs,
                Constants.EquipmentSlots.Feet,
                Constants.EquipmentSlots.Trinket1,
                Constants.EquipmentSlots.Trinket2,
                Constants.EquipmentSlots.Wield,
                Constants.EquipmentSlots.Wield2,
                Constants.EquipmentSlots.Wield3,
                Constants.EquipmentSlots.Wield4,
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
            AddAbility(1, Repository.AbilityManager.ThirdWieldAbility);
        }
    }
}
