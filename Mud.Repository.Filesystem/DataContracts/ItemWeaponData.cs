using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract]
    public class ItemWeaponData : ItemData
    {
        [DataMember]
        public int WeaponFlags { get; set; }
    }
}
