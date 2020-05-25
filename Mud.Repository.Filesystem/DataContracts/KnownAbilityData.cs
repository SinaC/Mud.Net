using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract(Namespace = "")]
    public class KnownAbilityData
    {
        [DataMember]
        public int AbilityId { get; set; }

        [DataMember]
        public int ResourceKind { get; set; }

        [DataMember]
        public int CostAmount { get; set; }

        [DataMember]
        public int CostAmountOperator { get; set; }

        [DataMember]
        public int Level { get; set; } // level at which ability can be learned

        [DataMember]
        public int Learned { get; set; } // practice percentage, 0 means not learned, 100 mean fully learned

        [DataMember]
        public int Rating { get; set; } // how difficult is it to improve/gain/practice
    }
}
