using System.Runtime.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract(Namespace = "")]
    [KnownType(typeof(AdminData))]
    public class PlayerData
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int PagingLineCount { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public PairData<string, string>[] Aliases { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public CharacterData[] Characters { get; set; }
    }
}
