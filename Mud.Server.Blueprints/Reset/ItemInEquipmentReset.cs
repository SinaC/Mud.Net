using Mud.Domain;

namespace Mud.Server.Blueprints.Reset;

public class ItemInEquipmentReset : ResetBase // 'E'
{
    public int ItemId { get; set; } // arg1
    public int GlobalLimit { get; set; } // arg2
    public EquipmentSlots EquipmentSlot { get; set; } // arg3
}
