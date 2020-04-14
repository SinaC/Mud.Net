using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract(Namespace = "")]
    public class EquipedItemData : ItemData
    {
        [DataMember]
        public int Slot { get; set; }
    }
}
