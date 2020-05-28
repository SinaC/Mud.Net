using System;
using System.Collections.Generic;

namespace Mud.Repository.Mongo.Domain
{
    public class PlayableCharacterData : CharacterData
    {
        public DateTime CreationTime { get; set; }

        public int RoomId { get; set; }

        public long SilverCoins { get; set; }

        public long GoldCoins { get; set; }

        public long Experience { get; set; }

        public int Trains { get; set; }

        public int Practices { get; set; }

        public int AutoFlags { get; set; }

        public CurrentQuestData[] CurrentQuests { get; set; }

        public KnownAbilityData[] KnownAbilities { get; set; }

        public Dictionary<int, int> Conditions { get; set; }

        public Dictionary<string,string> Aliases { get; set; }

        public Dictionary<int,int> Cooldowns { get; set; }

        public CharacterData[] Pets { get; set; }
    }
}
