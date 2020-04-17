using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract(Namespace = "")]
    public class ItemData
    {
        [DataMember]
        public int ItemId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<ItemData> Contains { get; set; }
    }
}
