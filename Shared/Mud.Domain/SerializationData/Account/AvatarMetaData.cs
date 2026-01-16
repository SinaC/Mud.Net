namespace Mud.Domain.SerializationData.Account;

public class AvatarMetaData
{
    public required string Name { get; set; }
    public required int Version { get; set; }
    public required int Level { get; set; }
    public required string Class { get; set; }
    public required string Race { get; set; }
    public required int RoomId { get; set; }
}
