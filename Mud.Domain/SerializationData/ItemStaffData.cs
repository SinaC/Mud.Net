using Mud.Domain.Serialization;

namespace Mud.Domain.SerializationData;

[JsonPolymorphism(typeof(ItemData), "staff")]
public class ItemStaffData : ItemCastSpellsChargeData
{
}
