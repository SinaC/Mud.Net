using Mud.Domain.SerializationData;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Entity;

namespace Mud.Server.Interfaces.Item;

public interface IItemLight : IItem
{
    void Initialize(Guid guid, ItemLightBlueprint blueprint, IContainer containedInto);
    void Initialize(Guid guid, ItemLightBlueprint blueprint, ItemLightData itemData, IContainer containedInto);

    bool IsLighten { get; }
    int TimeLeft { get; } // in minutes
    bool IsInfinite { get; }

    bool DecreaseTimeLeft();
}
