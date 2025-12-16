using Mud.Domain.Serialization;
using Mud.Domain.SerializationData;

namespace Mud.Server.Item;

[JsonBaseType(typeof(ItemData), "weapon")]
public class ItemWeaponData : ItemData
{
    public required string WeaponFlags { get; set; }
}
