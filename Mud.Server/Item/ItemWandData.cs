using Mud.Domain.Serialization;
using Mud.Domain.SerializationData;

namespace Mud.Server.Item;

[JsonPolymorphism(typeof(ItemData), "wand")]
public class ItemWandData : ItemCastSpellsChargeData
{
}
