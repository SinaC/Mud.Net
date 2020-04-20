using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract]
    public class CurrentQuestData
    {
        [DataMember]
        public int QuestId { get; set; }

        [DataMember]
        public DateTime StartTime { get; set; }

        [DataMember]
        public DateTime? CompletionTime { get; set; }

        [DataMember]
        public int GiverId { get; set; }

        [DataMember]
        public int GiverRoomId { get; set; }

        [DataMember]
        public List<CurrentQuestObjectiveData> Objectives { get; set; }
    }
}
