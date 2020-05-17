using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract]
    public class ItemLightData : ItemData
    {
        [DataMember]
        public int TimeLeft { get; set; }
    }
}
