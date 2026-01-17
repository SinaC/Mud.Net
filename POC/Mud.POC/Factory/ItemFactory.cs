namespace Mud.POC.Factory;

public class ItemFactory
{
    private IItemBlueprintManager ItemBlueprintManager { get; }

    public ItemFactory(IItemBlueprintManager itemBlueprintManager)
    {
        ItemBlueprintManager = itemBlueprintManager;
    }

    public IItem? CreateItem(ItemBlueprintBase blueprint)
    {
        var armor = new ItemArmor(); // TODO: use DI
        armor.Initialize(blueprint);

        return armor;
    }

    public IItem? CreateItem(ItemData itemData)
    {
        var blueprint = ItemBlueprintManager.GetItemBlueprint(itemData.BlueprintId);
        var armor = new ItemArmor(); // TODO: use DI
        armor.Initialize(blueprint, itemData);

        return armor;
    }
}

public abstract class EntityBase : IEntity
{
    public string Name { get; protected set; } = default!;

    public void Initialize(string name)
    {
        Name = name;
    }
}

public abstract class ItemBase : EntityBase
{
    public ItemBlueprintBase Blueprint { get; protected set; } = default!;
    public int Level { get; protected set; }

    public virtual void Initialize(ItemBlueprintBase itemBlueprintBase)
    {
        base.Initialize(itemBlueprintBase.Name);

        Blueprint = itemBlueprintBase;
        Level = itemBlueprintBase.Level;
    }

    public virtual void Initialize(ItemBlueprintBase itemBlueprintBase, ItemData itemData)
    {
        base.Initialize(itemData.Name);

        Blueprint = itemBlueprintBase;
        Level = itemData.Level;
    }
}

public class ItemArmor : ItemBase, IItemArmor
{
    public int Armor { get; protected set; }

    public override void Initialize(ItemBlueprintBase itemBlueprintBase)
    {
        var armorBlueprint = (ItemArmorBlueprint)itemBlueprintBase;
        base.Initialize(itemBlueprintBase);

        Armor = armorBlueprint.Armor;
    }

    public override void Initialize(ItemBlueprintBase itemBlueprintBase, ItemData itemData)
    {
        var armorBlueprint = (ItemArmorBlueprint)itemBlueprintBase;
        var armorData = (ItemArmorData)itemData;
        base.Initialize(itemBlueprintBase, itemData);

        Armor = armorData.Armor;
    }
}

public interface IEntity
{
    string Name { get; }
}

public interface IItem : IEntity
{
    ItemBlueprintBase Blueprint { get; }
    int Level { get; }
}

public interface IItemArmor : IItem
{
    int Armor { get; }
}

public abstract class ItemBlueprintBase
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required int Level { get; set; }
}

public class ItemArmorBlueprint : ItemBlueprintBase
{
    public required int Armor { get; set; }
}

public abstract class ItemData
{
    public required int BlueprintId { get; set; }
    public required string Name { get; set; }
    public required int Level { get; set; }
}

public class ItemArmorData : ItemData
{
    public required int Armor { get; set; }
}
