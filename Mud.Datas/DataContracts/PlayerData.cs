using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mud.Datas.DataContracts
{
    [DataContract]
    public class PlayerData
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<CoupledData<string, string>> Aliases { get; set; }

        [DataMember]
        public List<CharacterData> Characters { get; set; }
    }
}
