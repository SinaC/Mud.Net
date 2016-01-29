using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mud.Datas
{
    [DataContract]
    public class PlayerData
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string IsAdmin { get; set; }

        [DataMember]
        public Dictionary<string, string> Aliases { get; set; }

        [DataMember]
        public List<CharacterData> Characters { get; set; }
    }
}
