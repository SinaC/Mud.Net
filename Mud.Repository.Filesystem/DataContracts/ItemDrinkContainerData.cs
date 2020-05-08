using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract(Namespace = "")]
    public class ItemDrinkContainerData : ItemData
    {
        [DataMember]
        public int MaxLiquidAmount { get; set; }

        [DataMember]
        public int CurrentLiquidAmount { get; set; }

        [DataMember]
        public string LiquidName { get; set; }

        [DataMember]
        public bool IsPoisoned { get; set; }
    }
}
