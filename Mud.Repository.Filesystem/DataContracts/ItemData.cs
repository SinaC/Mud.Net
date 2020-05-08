using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract(Namespace = "")]
    [KnownType(typeof(ItemContainerData))]
    [KnownType(typeof(ItemCorpseData))]
    [KnownType(typeof(ItemWeaponData))]
    [KnownType(typeof(ItemDrinkContainerData))]
    [XmlInclude(typeof(ItemCorpseData))]
    [XmlInclude(typeof(ItemContainerData))]
    [XmlInclude(typeof(ItemWeaponData))]
    [XmlInclude(typeof(ItemDrinkContainerData))]
    public class ItemData
    {
        [DataMember]
        public int ItemId { get; set; }

        [DataMember]
        public int Level { get; set; }

        [DataMember]
        public int DecayPulseLeft { get; set; }

        [DataMember]
        public int ItemFlags { get; set; }

        [DataMember]
        public AuraData[] Auras { get; set; }
    }
}
