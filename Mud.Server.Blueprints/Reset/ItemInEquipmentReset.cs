using Mud.Domain;

namespace Mud.Server.Blueprints.Reset
{
    public class ItemInEquipmentReset : ResetBase
    {
        public int ItemId { get; set; }
        public int GlobalLimit { get; set; }
        public EquipmentSlots EquipmentSlot { get; set; }
    }
}
