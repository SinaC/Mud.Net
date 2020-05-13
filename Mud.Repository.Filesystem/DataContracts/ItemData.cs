using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract(Namespace = "")]
    [KnownType(typeof(ItemContainerData))]
    [KnownType(typeof(ItemCorpseData))]
    [KnownType(typeof(ItemWeaponData))]
    [KnownType(typeof(ItemDrinkContainerData))]
    [KnownType(typeof(ItemFoodData))]
    [KnownType(typeof(ItemPortalData))]
    [KnownType(typeof(ItemWandData))]
    [KnownType(typeof(ItemStaffData))]
    [XmlInclude(typeof(ItemCorpseData))]
    [XmlInclude(typeof(ItemContainerData))]
    [XmlInclude(typeof(ItemWeaponData))]
    [XmlInclude(typeof(ItemDrinkContainerData))]
    [XmlInclude(typeof(ItemFoodData))]
    [XmlInclude(typeof(ItemPortalData))]
    [XmlInclude(typeof(ItemWandData))]
    [XmlInclude(typeof(ItemStaffData))]
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
