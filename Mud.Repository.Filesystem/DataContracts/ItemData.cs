using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract(Namespace = "")]
    public class ItemData
    {
        [DataMember]
        public int ItemId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public ItemData[] Contains { get; set; }
    }
}
