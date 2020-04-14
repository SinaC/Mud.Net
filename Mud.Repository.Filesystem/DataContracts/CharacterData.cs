using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract(Namespace = "")]
    public class CharacterData
    {
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
        public List<EquipedItemData> Equipments { get; set; }

        [DataMember]
        public List<ItemData> Inventory { get; set; }

        // TODO: aura, cooldown, quests, ...
    }
}
