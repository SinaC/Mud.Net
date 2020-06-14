using System;
using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract(Namespace = "")]
    public class CurrentQuestData
    {
        [DataMember]
        public int QuestId { get; set; }

        [DataMember]
        public DateTime StartTime { get; set; }

        [DataMember]
        public int PulseLeft { get; set; }

        [DataMember]
        public DateTime? CompletionTime { get; set; }

        [DataMember]
        public int GiverId { get; set; }

        [DataMember]
        public int GiverRoomId { get; set; }

        [DataMember]
        public CurrentQuestObjectiveData[] Objectives { get; set; }
    }
}
