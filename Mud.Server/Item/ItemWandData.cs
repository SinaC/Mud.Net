using Mud.Domain.Serialization;
using Mud.Domain.SerializationData;

namespace Mud.Server.Item;

[JsonBaseType(typeof(ItemData), "wand")]
public class ItemWandData : ItemCastSpellsChargeData
{
}
