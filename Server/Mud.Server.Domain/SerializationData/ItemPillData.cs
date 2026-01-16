using Mud.Common.Attributes;
using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.Domain.SerializationData;

[JsonBaseType(typeof(ItemData), "pill")]
public class ItemPillData : ItemCastSpellsNoChargeData
{
}
