using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract]
    public class ItemWeaponFlagsAffectData : AffectDataBase
    {
        [DataMember]
        public int Operator { get; set; } // Add and Or are identical

        [DataMember]
        public int Modifier { get; set; }
    }
}
