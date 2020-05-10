using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract(Namespace = "")]
    public class ItemContainerData : ItemData
    {
        [DataMember]
        public int ContainerFlags { get; set; }

        [DataMember]
        public ItemData[] Contains { get; set; }
    }
}
