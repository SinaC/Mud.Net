using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract]
    public class ItemFoodData : ItemData
    {
        [DataMember]
        public bool IsPoisoned { get; set; }
    }
}
