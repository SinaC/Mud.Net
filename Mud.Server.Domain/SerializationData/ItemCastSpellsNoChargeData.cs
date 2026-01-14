using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.Domain.SerializationData;

public abstract class ItemCastSpellsNoChargeData : ItemData
{
    public required int SpellLevel { get; set; }
}
