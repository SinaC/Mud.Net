using Mud.Server.Flags.Interfaces;

namespace Mud.Domain
{
    public class ItemWeaponData : ItemData
    {
        public IWeaponFlags WeaponFlags { get; set; }
    }
}
