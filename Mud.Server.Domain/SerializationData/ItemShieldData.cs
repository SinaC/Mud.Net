using Mud.Common.Attributes;
using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.Domain.SerializationData;

[JsonBaseType(typeof(ItemData), "shield")]
public class ItemShieldData : ItemData
{
    public required int Armor { get; set; }
}
