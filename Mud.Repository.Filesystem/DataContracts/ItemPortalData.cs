using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract]
    public class ItemPortalData : ItemData
    {
        [DataMember]
        public int DestinationRoomId { get; set; }

        [DataMember]
        public int PortalFlags { get; set; }

        [DataMember]
        public int MaxChargeCount { get; set; }

        [DataMember]
        public int CurrentChargeCount { get; set; }
    }
}
