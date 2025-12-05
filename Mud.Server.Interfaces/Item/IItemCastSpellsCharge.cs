using Mud.Domain.SerializationData;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Entity;

namespace Mud.Server.Interfaces.Item;

public interface IItemCastSpellsCharge : IItem
{
    void Initialize(Guid guid, ItemCastSpellsChargeBlueprintBase blueprint, IContainer containedInto);
    void Initialize(Guid guid, ItemCastSpellsChargeBlueprintBase blueprint, ItemCastSpellsChargeData data, IContainer containedInto);

    int SpellLevel { get; }
    int MaxChargeCount { get; }
    int CurrentChargeCount { get; }
    string SpellName { get; }
    bool AlreadyRecharged { get; }

    void Use();
    void Recharge(int currentChargeCount, int maxChargeCount);
}
