using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract(Namespace = "")]
    public class PetData : CharacterData
    {
        [DataMember]
        public int BlueprintId { get; set; }
    }
}
