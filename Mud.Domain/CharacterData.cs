﻿using System;
using System.Collections.Generic;

namespace Mud.Domain
{
    public class CharacterData
    {
        public DateTime CreationTime { get; set; }

        public string Name { get; set; }

        public int RoomId { get; set; }

        public string Race { get; set; }

        public string Class { get; set; }

        public int Level { get; set; }

        public Sex Sex { get; set; }

        public Sizes Size { get; set; }

        public long SilverCoins { get; set; }

        public long GoldCoins { get; set; }

        public int HitPoints { get; set; }

        public int MovePoints { get; set; }

        public Dictionary<ResourceKinds, int> CurrentResources { get; set; }

        public Dictionary<ResourceKinds, int> MaxResources { get; set; }

        public long Experience { get; set; }

        public int Trains { get; set; }

        public int Practices { get; set; }

        public EquippedItemData[] Equipments { get; set; }

        public ItemData[] Inventory { get; set; }

        public CurrentQuestData[] CurrentQuests { get; set; }

        public AuraData[] Auras { get; set; }

        public CharacterFlags CharacterFlags { get; set; }

        public IRVFlags Immunities { get; set; }

        public IRVFlags Resistances { get; set; }

        public IRVFlags Vulnerabilities { get; set; }

        public Dictionary<CharacterAttributes, int> Attributes { get; set; }

        public KnownAbilityData[] KnownAbilities { get; set; }

        public Dictionary<Conditions, int> Conditions { get; set; }

        public Dictionary<string, string> Aliases { get; set; }

        public Dictionary<int, int> Cooldowns { get; set; }
    }
}
