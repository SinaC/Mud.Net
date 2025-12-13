using Mud.Domain.SerializationData;

namespace Mud.Server.Item;

public class ItemCastSpellsChargeData : ItemData
{
    public required int MaxChargeCount { get; set; }
    public required int CurrentChargeCount { get; set; }
    public required bool AlreadyRecharged { get; set; }
}
