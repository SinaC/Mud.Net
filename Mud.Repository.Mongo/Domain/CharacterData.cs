using System;
using System.Collections.Generic;

namespace Mud.Repository.Mongo.Domain
{
    public class CharacterData
    {
        public DateTime CreationTime { get; set; }

        public string Name { get; set; }

        public int RoomId { get; set; }

        public string Race { get; set; }

        public string Class { get; set; }

        public int Level { get; set; }

        public int Sex { get; set; }

        public long Experience { get; set; }

        public List<EquipedItemData> Equipments { get; set; }

        public List<ItemData> Inventory { get; set; }

        public List<CurrentQuestData> CurrentQuests { get; set; }

        // TODO: aura, cooldown, ...
    }
}
