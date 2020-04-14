using System.Collections.Generic;

namespace Mud.Domain
{
    public class PlayerData
    {
        public string Name { get; set; }

        public Dictionary<string, string> Aliases { get; set; }

        public List<CharacterData> Characters { get; set; }
    }
}
