using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract(Namespace = "")]
    [KnownType(typeof(PlayableCharacterData))]
    [KnownType(typeof(PetData))]
    [XmlInclude(typeof(PlayableCharacterData))]
    [XmlInclude(typeof(PetData))]
    public abstract class CharacterData
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Race { get; set; }

        [DataMember]
        public string Class { get; set; }

        [DataMember]
        public int Level { get; set; }

        [DataMember]
        public int Sex { get; set; }

        [DataMember]
        public int Size { get; set; }

        [DataMember]
        public int HitPoints { get; set; }

        [DataMember]
        public int MovePoints { get; set; }

        [DataMember]
        public PairData<int, int>[] CurrentResources { get; set; }

        [DataMember]
        public PairData<int, int>[] MaxResources { get; set; }

        [DataMember]
        public EquippedItemData[] Equipments { get; set; }

        [DataMember]
        public ItemData[] Inventory { get; set; }

        [DataMember]
        public AuraData[] Auras { get; set; }

        [DataMember]
        public int CharacterFlags { get; set; }

        [DataMember]
        public int Immunities { get; set; }

        [DataMember]
        public int Resistances { get; set; }

        [DataMember]
        public int Vulnerabilities { get; set; }

        [DataMember]
        public PairData<int,int>[] Attributes { get; set; }
    }
}
