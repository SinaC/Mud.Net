using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract(Namespace = "")]
    public class EquippedItemData
    {
        [DataMember]
        public int Slot { get; set; }

        [DataMember]
        public ItemData Item { get; set; }
    }
}
