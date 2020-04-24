using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract(Namespace = "")]
    public class ItemContainerData : ItemData
    {
        [DataMember]
        public ItemData[] Contains { get; set; }
    }
}
