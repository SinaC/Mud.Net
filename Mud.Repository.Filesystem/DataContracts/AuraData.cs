using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract]
    public class AuraData
    {
        [DataMember]
        public int AbilitiId { get; set; }

        // TODO: source

        [DataMember]
        public int Modifier { get; set; }

        [DataMember]
        public int Amount { get; set; }

        [DataMember]
        public int AmountOperator { get; set; }

        [DataMember]
        public int Level { get; set; }

        [DataMember]
        public int PulseLeft { get; set; }
    }
}
