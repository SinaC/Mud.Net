using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract]
    public class ItemFoodData : ItemData
    {
        [DataMember]
        public int FullHours { get; set; }
        [DataMember]
        public int HungerHours { get; set; }
        [DataMember]
        public bool IsPoisoned { get; set; }
    }
}
