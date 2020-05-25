using System;
using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract(Namespace = "")]
    public class PlayableCharacterData : CharacterData
    {
        [DataMember]
        public DateTime CreationTime { get; set; }

        [DataMember]
        public int RoomId { get; set; }

        [DataMember]
        public long SilverCoins { get; set; }

        [DataMember]
        public long GoldCoins { get; set; }

        [DataMember]
        public long Experience { get; set; }

        [DataMember]
        public int Trains { get; set; }

        [DataMember]
        public int Practices { get; set; }

        [DataMember]
        public CurrentQuestData[] CurrentQuests { get; set; }

        [DataMember]
        public KnownAbilityData[] KnownAbilities { get; set; }

        [DataMember]
        public PairData<int,int>[] Conditions { get; set; }

        [DataMember]
        public PairData<string, string>[] Aliases { get; set; }

        [DataMember]
        public PairData<int,int>[] Cooldowns { get; set; }

        [DataMember]
        public CharacterData[] Pets { get; set; }
    }
}
