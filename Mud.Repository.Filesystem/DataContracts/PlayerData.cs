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
        public List<PairData<string, string>> Aliases { get; set; }

        [DataMember]
        public List<CharacterData> Characters { get; set; }
    }
}
