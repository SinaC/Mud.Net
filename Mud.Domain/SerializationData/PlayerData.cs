using Mud.Domain.Serialization;

namespace Mud.Domain.SerializationData;

[JsonBaseType(typeof(PlayerData), "player")]
public class PlayerData
{
    public required string Name { get; set; }

    public required int PagingLineCount { get; set; }

    public required Dictionary<string, string> Aliases { get; set; }

    public required PlayableCharacterData[] Characters { get; set; }
}
