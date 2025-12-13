using System.Text.Json.Serialization;

namespace Mud.POC.Tests.Serialization;

public class PlayerData
{
    public string Name { get; set; }

    public int PagingLineCount { get; set; }

    public Dictionary<string, string> Aliases { get; set; }

    public PlayableCharacterData[] Characters { get; set; }
}
