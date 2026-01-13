using Mud.Common.Attributes;
using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.Item.SerializationData;

[JsonBaseType(typeof(ItemData), "weapon")]
public class ItemWeaponData : ItemData
{
    public required string WeaponFlags { get; set; }
}
