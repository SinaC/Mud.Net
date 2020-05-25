﻿using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract(Namespace = "")]
    public class ItemWeaponData : ItemData
    {
        [DataMember]
        public int WeaponFlags { get; set; }
    }
}
