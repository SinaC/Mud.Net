using Mud.Common.Attributes;
using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.Item;

[JsonBaseType(typeof(ItemData), "wand")]
public class ItemWandData : ItemCastSpellsChargeData
{
}
