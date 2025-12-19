namespace Mud.Server.Domain;

public record SocialDefinition(string Name, string? CharacterNoArg, string? OthersNoArg, string? CharacterFound, string? OthersFound, string? VictimFound, string? CharacterNotFound, string? CharacterAuto, string? OthersAuto);
