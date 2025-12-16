using Mud.Domain.Serialization;
using Mud.Domain.SerializationData;

namespace Mud.Server.Item;

[JsonBaseType(typeof(ItemData), "staff")]
public class ItemStaffData : ItemCastSpellsChargeData
{
}
