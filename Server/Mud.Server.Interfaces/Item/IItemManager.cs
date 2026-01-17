using Mud.Blueprints.Item;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Room;
using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.Interfaces.Item;

public interface IItemManager
{
    IReadOnlyCollection<ItemBlueprintBase> ItemBlueprints { get; }

    ItemBlueprintBase? GetItemBlueprint(int id);
    TBlueprint? GetItemBlueprint<TBlueprint>(int id)
        where TBlueprint : ItemBlueprintBase;

    void AddItemBlueprint(ItemBlueprintBase blueprint);

    IEnumerable<IItem> Items { get; }
    int Count(int blueprintId);

    IItem? AddItem(Guid guid, ItemBlueprintBase blueprint, string source, IContainer container);
    IItem? AddItem(Guid guid, ItemData itemData, IContainer container);
    IItem? AddItem(Guid guid, int blueprintId, string source, IContainer container);
    TItem? AddItem<TItem>(Guid guid, int blueprintId, string source, IContainer container)
        where TItem : class, IItem;

    IItemCorpse? AddItemCorpse(Guid guid, ICharacter victim, string source, IRoom room);
    IItemMoney? AddItemMoney(Guid guid, long silverCoins, long goldCoins, string source, IContainer container);

    void RemoveItem(IItem item);

    void Cleanup();
}
