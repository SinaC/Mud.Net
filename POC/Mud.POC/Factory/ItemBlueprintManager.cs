namespace Mud.POC.Factory;

public class ItemBlueprintManager : IItemBlueprintManager
{
    private List<ItemBlueprintBase> ItemBlueprints { get; } = [];

    public ItemBlueprintBase? GetItemBlueprint(int id)
        => ItemBlueprints.FirstOrDefault(x => x.Id == id);

    public TBlueprint? GetItemBlueprint<TBlueprint>(int id)
        where TBlueprint : ItemBlueprintBase
        => ItemBlueprints.OfType<TBlueprint>().FirstOrDefault(x => x.Id == id);

    public void AddItemBlueprint(ItemBlueprintBase blueprint)
        => ItemBlueprints.Add(blueprint);
}

public interface IItemBlueprintManager
{
    ItemBlueprintBase? GetItemBlueprint(int id);
    TBlueprint? GetItemBlueprint<TBlueprint>(int id)
        where TBlueprint : ItemBlueprintBase;

    void AddItemBlueprint(ItemBlueprintBase blueprint);
}
