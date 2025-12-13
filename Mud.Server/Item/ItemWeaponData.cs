using Mud.Domain.Serialization;
using Mud.Domain.SerializationData;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Item;

[JsonPolymorphism(typeof(ItemData), "weapon")]
public class ItemWeaponData : ItemData
{
    public required string WeaponFlags { get; set; }
}
