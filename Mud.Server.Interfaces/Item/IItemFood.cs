using Mud.Domain.SerializationData;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Entity;

namespace Mud.Server.Interfaces.Item;

public interface IItemFood : IItemPoisonable
{
    void Initialize(Guid guid, ItemFoodBlueprint blueprint, IContainer containedInto);
    void Initialize(Guid guid, ItemFoodBlueprint blueprint, ItemFoodData data, IContainer containedInto);

    int FullHours { get; }

    int HungerHours { get; }

    void SetHours(int fullHours, int hungerHours);
}
