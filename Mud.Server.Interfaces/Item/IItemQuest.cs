using Mud.Domain.SerializationData;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Entity;

namespace Mud.Server.Interfaces.Item;

public interface IItemQuest : IItem
{
    void Initialize(Guid guid, ItemQuestBlueprint blueprint, IContainer containedInto);
    void Initialize(Guid guid, ItemQuestBlueprint blueprint, ItemData itemData, IContainer containedInto);
    // No additional datas
}
