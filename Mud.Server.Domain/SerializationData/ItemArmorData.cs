using Mud.Common.Attributes;
using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.Domain.SerializationData;

[JsonBaseType(typeof(ItemData), "armor")]
public class ItemArmorData : ItemData
{
    public required int Bash { get; set; }
    public required int Pierce { get; set; }
    public required int Slash { get; set; }
    public required int Exotic { get; set; }
}
