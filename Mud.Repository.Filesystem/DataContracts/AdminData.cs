using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract(Namespace = "")]
    public class AdminData : PlayerData
    {
        [DataMember]
        public int AdminLevel { get; set; }

        [DataMember]
        public int WiznetFlags { get; set; }

        // TODO: extra fields
    }
}
