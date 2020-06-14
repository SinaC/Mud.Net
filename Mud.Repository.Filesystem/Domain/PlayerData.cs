using System.Xml.Serialization;

namespace Mud.Repository.Filesystem.Domain
{
    [XmlInclude(typeof(AdminData))]
    public class PlayerData
    {
        public string Name { get; set; }

        public int PagingLineCount { get; set; }

        public PairData<string, string>[] Aliases { get; set; }

        public PlayableCharacterData[] Characters { get; set; }
    }
}
