using Mud.Domain.SerializationData;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Entity;

namespace Mud.Server.Interfaces.Item;

public interface IItemCastSpellsNoCharge : IItem
{
    void Initialize(Guid guid, ItemCastSpellsNoChargeBlueprintBase blueprint, IContainer containedInto);
    void Initialize(Guid guid, ItemCastSpellsNoChargeBlueprintBase blueprint, ItemData data, IContainer containedInto);

    int SpellLevel { get; }
    string FirstSpellName { get; }
    string SecondSpellName { get; }
    string ThirdSpellName { get; }
    string FourthSpellName { get; }
}
