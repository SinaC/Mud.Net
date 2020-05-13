using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract]
    public abstract class ItemCastSpellsChargeData : ItemData
    {
        [DataMember]
        public int MaxChargeCount { get; set; }

        [DataMember]
        public int CurrentChargeCount { get; set; }

        [DataMember]
        public bool AlreadyRecharged { get; set; }
    }
}
