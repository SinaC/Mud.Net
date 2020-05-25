using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Mud.Repository.Filesystem.DataContracts
{
    [DataContract(Namespace = "")]
    [KnownType(typeof(AdminData))]
    [XmlInclude(typeof(AdminData))]
    public class PlayerData
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int PagingLineCount { get; set; }

        [DataMember]
        public PairData<string, string>[] Aliases { get; set; }

        [DataMember]
        public PlayableCharacterData[] Characters { get; set; }
    }
}
