using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Interfaces.Item;

public interface IItemManager
{
    IReadOnlyCollection<ItemBlueprintBase> ItemBlueprints { get; }

    ItemBlueprintBase? GetItemBlueprint(int id);
    TBlueprint? GetItemBlueprint<TBlueprint>(int id)
        where TBlueprint : ItemBlueprintBase;

    void AddItemBlueprint(ItemBlueprintBase blueprint);

    IEnumerable<IItem> Items { get; }

    IItem? AddItem(Guid guid, ItemBlueprintBase blueprint, IContainer container);
    IItem? AddItem(Guid guid, ItemData itemData, IContainer container);
    IItem? AddItem(Guid guid, int blueprintId, IContainer container);
    TItem? AddItem<TItem>(Guid guid, int blueprintId, IContainer container)
        where TItem : class, IItem;

    IItemCorpse? AddItemCorpse(Guid guid, IRoom room, ICharacter victim);
    IItemCorpse? AddItemCorpse(Guid guid, IRoom room, ICharacter victim, ICharacter killer);
    IItemMoney? AddItemMoney(Guid guid, long silverCoins, long goldCoins, IContainer container);

    void RemoveItem(IItem item);

    void Cleanup();
}
