using Mud.Domain.Serialization;

namespace Mud.Domain.SerializationData;

[JsonPolymorphism(typeof(ItemData), "wand")]
public class ItemWandData : ItemCastSpellsChargeData
{
}
