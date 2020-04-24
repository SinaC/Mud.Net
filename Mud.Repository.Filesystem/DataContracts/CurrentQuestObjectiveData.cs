using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract(Namespace = "")]
    public class CurrentQuestObjectiveData
    {
        [DataMember]
        public int ObjectiveId { get; set; }

        [DataMember]
        public int Count { get; set; }
    }
}
