using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Entity;

namespace Mud.Server.Interfaces.Item;

public interface IItemMoney : IItem
{
    void Initialize(Guid guid, ItemMoneyBlueprint blueprint, IContainer containedInto);
    void Initialize(Guid guid, ItemMoneyBlueprint blueprint, ItemData data, IContainer containedInto);
    void Initialize(Guid guid, ItemMoneyBlueprint blueprint, long silverCoins, long goldCoins, IContainer containedInto);

    long SilverCoins { get; }
    long GoldCoins { get; }
}
