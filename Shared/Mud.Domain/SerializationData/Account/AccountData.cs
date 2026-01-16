namespace Mud.Domain.SerializationData.Account;

public class AccountData
{
    public required int Version { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required int PagingLineCount { get; set; }
    public required Dictionary<string, string> Aliases { get; set; }

    public required AdminData? AdminData { get; set; }
    public required AvatarMetaData[] AvatarMetaDatas { get; set; }
}
