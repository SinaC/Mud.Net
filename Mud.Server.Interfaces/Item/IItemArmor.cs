using Mud.Domain.SerializationData;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Entity;

namespace Mud.Server.Interfaces.Item;

public interface IItemArmor : IItem
{
    void Initialize(Guid guid, ItemArmorBlueprint blueprint, IContainer containedInto);
    void Initialize(Guid guid, ItemArmorBlueprint blueprint, ItemData itemData, IContainer containedInto);

    int Bash { get; }
    int Pierce { get; }
    int Slash { get; }
    int Exotic { get; }
}
