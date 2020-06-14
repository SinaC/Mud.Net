using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract(Namespace = "")]
    public class EquipedItemData
    {
        [DataMember]
        public int Slot { get; set; }

        [DataMember]
        public ItemData Item { get; set; }
    }
}
