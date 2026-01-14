using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.Domain.SerializationData;

public abstract class ItemCastSpellsChargeData : ItemData
{
    public required int SpellLevel { get; set; }
    public required int MaxChargeCount { get; set; }
    public required int CurrentChargeCount { get; set; }
    public required bool AlreadyRecharged { get; set; }
}
