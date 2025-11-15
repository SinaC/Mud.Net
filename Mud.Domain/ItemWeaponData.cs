using Mud.Server.Flags.Interfaces;

namespace Mud.Domain;

public class ItemWeaponData : ItemData
{
    public required IWeaponFlags WeaponFlags { get; set; }
}
