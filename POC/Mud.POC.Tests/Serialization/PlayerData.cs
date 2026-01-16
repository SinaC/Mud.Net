namespace Mud.POC.Tests.Serialization;

public class PlayerData
{
    public string Name { get; set; } = null!;

    public int PagingLineCount { get; set; }

    public Dictionary<string, string> Aliases { get; set; } = null!;

    public PlayableCharacterData[] Characters { get; set; } = null!;
}
