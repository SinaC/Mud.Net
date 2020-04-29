using System;

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

        public EquipedItemData[] Equipments { get; set; }

        public ItemData[] Inventory { get; set; }

        public CurrentQuestData[] CurrentQuests { get; set; }

        public AuraData[] Auras { get; set; }

        // TODO: cooldown, ...
    }
}
