using System.Collections.Generic;

namespace Mud.Domain
{
    public class PlayerData
    {
        public string Name { get; set; }

        public int PagingLineCount { get; set; }

        public Dictionary<string, string> Aliases { get; set; }

        public CharacterData[] Characters { get; set; }
    }
}
