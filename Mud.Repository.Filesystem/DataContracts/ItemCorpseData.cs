using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract(Namespace = "")]
    public class ItemCorpseData : ItemData
    {
        [DataMember]
        public bool IsPlayableCharacterCorpse { get; set; }

        [DataMember]
        public string CorpseName { get; set; }

        [DataMember]
        public ItemData[] Contains { get; set; }

        [DataMember]
        public bool HasBeenGeneratedByKillingCharacter { get; set; }
    }
}
