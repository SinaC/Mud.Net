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
        public int Level { get; set; }

        [DataMember]
        public int PulseLeft { get; set; }

        [DataMember]
        public int AuraFlags { get; set; }

        [DataMember]
        public AffectDataBase[] Affects { get; set; }
    }
}
