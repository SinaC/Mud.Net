using Mud.POC.Factory;

namespace Mud.POC.Tests.Factory;

[TestClass]
public class ItemFactoryTests
{
    [TestMethod]
    public void CreateArmorFromBlueprintId()
    {
        var armorBlueprint = new ItemArmorBlueprint { Id = 1, Name = "breastplate", Level = 7, Armor = 15 };
        var itemBlueprintManager = new ItemBlueprintManager();
        itemBlueprintManager.AddItemBlueprint(armorBlueprint);

        var factory = new ItemFactory(itemBlueprintManager);

        var armor = factory.CreateItem(armorBlueprint);

        Assert.IsNotNull(armor);
        Assert.IsInstanceOfType<IItemArmor>(armor);
        Assert.AreEqual("breastplate", armor.Name);
        Assert.AreEqual(7, ((IItemArmor)armor).Level);
        Assert.AreEqual(15, ((IItemArmor)armor).Armor);
    }

    [TestMethod]
    public void CreateArmorFromItemData()
    {
        var armorBlueprint = new ItemArmorBlueprint { Id = 1, Name = "breastplate", Level = 7, Armor = 15 };
        var itemBlueprintManager = new ItemBlueprintManager();
        itemBlueprintManager.AddItemBlueprint(armorBlueprint);

        var armorData = new ItemArmorData { BlueprintId = 1, Name = "breastplate", Level = 9, Armor = 23 };

        var factory = new ItemFactory(itemBlueprintManager);

        var armor = factory.CreateItem(armorData);

        Assert.IsNotNull(armor);
        Assert.IsInstanceOfType<IItemArmor>(armor);
        Assert.AreEqual("breastplate", armor.Name);
        Assert.AreEqual(9, ((IItemArmor)armor).Level);
        Assert.AreEqual(23, ((IItemArmor)armor).Armor);
    }
}
