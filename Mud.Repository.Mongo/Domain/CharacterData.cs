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

        public int HitPoints { get; set; }

        public int MovePoints { get; set; }

        public Dictionary<int, int> CurrentResources { get; set; }

        public Dictionary<int, int> MaxResources { get; set; }

        public long Experience { get; set; }

        public int Trains { get; set; }

        public int Practices { get; set; }

        public EquipedItemData[] Equipments { get; set; }

        public ItemData[] Inventory { get; set; }

        public CurrentQuestData[] CurrentQuests { get; set; }

        public AuraData[] Auras { get; set; }

        public int CharacterFlags { get; set; }

        public int Immunities { get; set; }

        public int Resistances { get; set; }

        public int Vulnerabilities { get; set; }

        public Dictionary<int, int> Attributes { get; set; } // TODO: this could create duplicate key exception while deserializing if CharacterAttribute is not found anymore

        public KnownAbilityData[] KnownAbilities { get; set; }

        // TODO: cooldown, ...
    }
}
