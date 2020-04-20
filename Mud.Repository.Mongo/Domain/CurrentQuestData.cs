using System;
using System.Collections.Generic;

namespace Mud.Repository.Mongo.Domain
{
    public class CurrentQuestData
    {
        public int QuestId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? CompletionTime { get; set; }

        public int GiverId { get; set; }

        public int GiverRoomId { get; set; }

        public List<CurrentQuestObjectiveData> Objectives { get; set; }
    }
}
