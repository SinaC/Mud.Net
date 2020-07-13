using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
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

        public int Alignment { get; set; }

        public int Trains { get; set; }

        public int Practices { get; set; }

        public int AutoFlags { get; set; }

        public CurrentQuestData[] CurrentQuests { get; set; }

        public LearnedAbilityData[] LearnedAbilities { get; set; }

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, int> Conditions { get; set; }

        public Dictionary<string,string> Aliases { get; set; }

        public Dictionary<string, int> Cooldowns { get; set; }

        public CharacterData[] Pets { get; set; }
    }
}
