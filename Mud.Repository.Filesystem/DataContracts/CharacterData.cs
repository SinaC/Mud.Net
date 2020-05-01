using System;
using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract(Namespace = "")]
    public class CharacterData
    {
        [DataMember]
        public DateTime CreationTime { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int RoomId { get; set; }

        [DataMember]
        public string Race { get; set; }

        [DataMember]
        public string Class { get; set; }

        [DataMember]
        public int Level { get; set; }

        [DataMember]
        public int Sex { get; set; }

        [DataMember]
        public long Experience { get; set; }

        [DataMember]
        public EquipedItemData[] Equipments { get; set; }

        [DataMember]
        public ItemData[] Inventory { get; set; }

        [DataMember]
        public CurrentQuestData[] CurrentQuests { get; set; }

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
        public PairData<int,int>[] Attributes { get; set; } // TODO: this could create duplicate key exception while deserializing if CharacterAttribute is not found anymore

        // TODO: cooldown, ...
    }
}
