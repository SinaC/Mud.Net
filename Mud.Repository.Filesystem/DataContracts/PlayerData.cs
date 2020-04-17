using System.Collections.Generic;
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
        public List<PairData<string, string>> Aliases { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<CharacterData> Characters { get; set; }
    }
}
