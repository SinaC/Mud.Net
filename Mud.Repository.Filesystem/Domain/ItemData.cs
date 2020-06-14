using System.Xml.Serialization;

namespace Mud.Repository.Filesystem.Domain
{
    [XmlInclude(typeof(ItemCorpseData))]
    [XmlInclude(typeof(ItemContainerData))]
    [XmlInclude(typeof(ItemWeaponData))]
    [XmlInclude(typeof(ItemDrinkContainerData))]
    [XmlInclude(typeof(ItemFoodData))]
    [XmlInclude(typeof(ItemPortalData))]
    [XmlInclude(typeof(ItemWandData))]
    [XmlInclude(typeof(ItemStaffData))]
    [XmlInclude(typeof(ItemLightData))]
    public class ItemData
    {
        public int ItemId { get; set; }

        public int Level { get; set; }

        public int DecayPulseLeft { get; set; }

        public int ItemFlags { get; set; }

        public AuraData[] Auras { get; set; }
    }
}
