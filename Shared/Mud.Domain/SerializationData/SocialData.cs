namespace Mud.Domain.SerializationData;

public class SocialData
{
    public required string Name { get; set; }
    public required string? CharacterNoArg { get; set; }
    public required string? OthersNoArg { get; set; }
    public required string? CharacterFound { get; set; }
    public required string? OthersFound { get; set; }
    public required string? VictimFound { get; set; }
    public required string? CharacterNotFound { get; set; }
    public required string? CharacterAuto { get; set; }
    public required string? OthersAuto { get; set; }
}
