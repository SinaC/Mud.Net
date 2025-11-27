using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Entity;

namespace Mud.Server.Interfaces.Item;

public interface IItemShield : IItem
{
    void Initialize(Guid guid, ItemShieldBlueprint blueprint, IContainer containedInto);
    void Initialize(Guid guid, ItemShieldBlueprint blueprint, ItemData itemData, IContainer containedInto);

    int Armor { get; }
    // TODO: resistances
}
