using Mud.Domain.Serialization;
using Mud.Domain.SerializationData;

namespace Mud.Server.Item;

[JsonPolymorphism(typeof(ItemData), "staff")]
public class ItemStaffData : ItemCastSpellsChargeData
{
}
