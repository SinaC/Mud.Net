using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.Item;

public class ItemCastSpellsChargeData : ItemData
{
    public required int MaxChargeCount { get; set; }
    public required int CurrentChargeCount { get; set; }
    public required bool AlreadyRecharged { get; set; }
}
