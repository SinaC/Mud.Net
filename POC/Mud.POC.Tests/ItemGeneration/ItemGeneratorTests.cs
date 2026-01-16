using Moq;
using Mud.Blueprints.Item;
using Mud.Domain;
using Mud.Flags;
using Mud.POC.ItemGeneration;
using Mud.Random;
using Mud.Server.Affects.Character;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using System.Collections;

namespace Mud.POC.Tests.ItemGeneration;

[TestClass]
public class ItemGeneratorTests
{
    [TestMethod]
    public void OneStat_Common()
    {
        var randomManagerMock = new Mock<IRandomManager>();
        var itemManagerMock = new Mock<IItemManager>();
        var roomMock = new Mock<IContainer>();

        var itemBlueprint = new ItemArmorBlueprint
        {
            Id = 98,
            Name = "breastplate",
            ShortDescription = "a breastplate",
            Description = "a breastplate is here.",
            WearLocation = WearLocations.Chest,
            Level = 0,
            Cost = 0,
            NoTake = false,
            ItemFlags = new ItemFlags(),
            Bash = 0,
            Pierce = 0,
            Slash = 0,
            Exotic = 0,
            Weight = 5,
        };
        var itemMock = new Mock<IItem>();
        itemMock.SetupGet(x => x.Blueprint).Returns(itemBlueprint);

        randomManagerMock.Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((int min, int max) => 1); // 1 -> common
        randomManagerMock.Setup(x => x.Random(It.IsAny<IEnumerable<It.IsAnyType>>()))
            .Returns((IEnumerable list) => GetFirst(list));
        itemManagerMock.Setup(x => x.AddItem(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IContainer>()))
            .Returns(itemMock.Object);

        var generator = new ItemGenerator(randomManagerMock.Object, itemManagerMock.Object);
        generator.AddBlueprintId(98);

        var item = generator.Generate(25, roomMock.Object);

        Assert.IsNotNull(item);
        // modifier = 25 * 1 * 1 / (1 * 4) = 6
        itemMock.Verify(x => x.AddAura(It.Is<IAura>(x => 
                x.Affects.Count() == 1
                && x.Affects.OfType<CharacterAttributeAffect>().Single().Location == CharacterAttributeAffectLocations.Strength
                && x.Affects.OfType<CharacterAttributeAffect>().Single().Modifier == 6),
            It.IsAny<bool>()), Times.Once);
        itemMock.Verify(x => x.SetLevel(25), Times.Once);
        itemMock.Verify(x => x.SetCost(10*25*100/100), Times.Once);
        itemMock.Verify(x => x.SetShortDescription("a breastplate of strength"), Times.Once);
        itemMock.Verify(x => x.SetDescription("A Breastplate of Strength is here."), Times.Once);
    }

    [TestMethod]
    public void OneStat_Uncommon()
    {
        var randomManagerMock = new Mock<IRandomManager>();
        var itemManagerMock = new Mock<IItemManager>();
        var roomMock = new Mock<IContainer>();

        var itemBlueprint = new ItemArmorBlueprint
        {
            Id = 98,
            Name = "breastplate",
            ShortDescription = "a breastplate",
            Description = "a breastplate is here",
            WearLocation = WearLocations.Chest,
            Level = 0,
            Cost = 0,
            NoTake = false,
            ItemFlags = new ItemFlags(),
            Bash = 0,
            Pierce = 0,
            Slash = 0,
            Exotic = 0,
            Weight = 5,
        };
        var itemMock = new Mock<IItem>();
        itemMock.SetupGet(x => x.Blueprint).Returns(itemBlueprint);

        randomManagerMock.Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((int min, int max) => 75); // 75 -> ucommon
        randomManagerMock.Setup(x => x.Random(It.IsAny<IEnumerable<It.IsAnyType>>()))
            .Returns((IEnumerable list) => GetFirst(list));
        itemManagerMock.Setup(x => x.AddItem(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IContainer>()))
            .Returns(itemMock.Object);

        var generator = new ItemGenerator(randomManagerMock.Object, itemManagerMock.Object);
        generator.AddBlueprintId(98);

        var item = generator.Generate(25, roomMock.Object);

        Assert.IsNotNull(item);
        // modifier = 25 * 1.5 * 1 / (1 * 4) = 9
        itemMock.Verify(x => x.AddAura(It.Is<IAura>(x =>
                x.Affects.Count() == 1
                && x.Affects.OfType<CharacterAttributeAffect>().Single().Location == CharacterAttributeAffectLocations.Strength
                && x.Affects.OfType<CharacterAttributeAffect>().Single().Modifier == 9),
            It.IsAny<bool>()), Times.Once);
        itemMock.Verify(x => x.SetLevel(25), Times.Once);
        itemMock.Verify(x => x.SetCost(10 * 25 * 150 / 100), Times.Once);
        itemMock.Verify(x => x.SetShortDescription("a breastplate of strength"), Times.Once);
        itemMock.Verify(x => x.SetDescription("A Breastplate of Strength is here."), Times.Once);
    }

    [TestMethod]
    public void OneStat_Rare()
    {
        var randomManagerMock = new Mock<IRandomManager>();
        var itemManagerMock = new Mock<IItemManager>();
        var roomMock = new Mock<IContainer>();

        var itemBlueprint = new ItemArmorBlueprint
        {
            Id = 98,
            Name = "breastplate",
            ShortDescription = "a breastplate",
            Description = "a breastplate is here",
            WearLocation = WearLocations.Chest,
            Level = 0,
            Cost = 0,
            NoTake = false,
            ItemFlags = new ItemFlags(),
            Bash = 0,
            Pierce = 0,
            Slash = 0,
            Exotic = 0,
            Weight = 5,
        };
        var itemMock = new Mock<IItem>();
        itemMock.SetupGet(x => x.Blueprint).Returns(itemBlueprint);

        randomManagerMock.Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((int min, int max) => 99); // 99 -> rare
        randomManagerMock.Setup(x => x.Random(It.IsAny<IEnumerable<It.IsAnyType>>()))
            .Returns((IEnumerable list) => GetFirst(list));
        itemManagerMock.Setup(x => x.AddItem(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IContainer>()))
            .Returns(itemMock.Object);

        var generator = new ItemGenerator(randomManagerMock.Object, itemManagerMock.Object);
        generator.AddBlueprintId(98);

        var item = generator.Generate(25, roomMock.Object);

        Assert.IsNotNull(item);
        // modifier = 25 * 2 * 1 / (1 * 4) = 12
        itemMock.Verify(x => x.AddAura(It.Is<IAura>(x =>
                x.Affects.Count() == 1
                && x.Affects.OfType<CharacterAttributeAffect>().Single().Location == CharacterAttributeAffectLocations.Strength
                && x.Affects.OfType<CharacterAttributeAffect>().Single().Modifier == 12),
            It.IsAny<bool>()), Times.Once);
        itemMock.Verify(x => x.SetLevel(25), Times.Once);
        itemMock.Verify(x => x.SetCost(10 * 25 * 200 / 100), Times.Once);
        itemMock.Verify(x => x.SetShortDescription("a breastplate of strength"), Times.Once);
        itemMock.Verify(x => x.SetDescription("A Breastplate of Strength is here."), Times.Once);
    }

    [TestMethod]
    public void TwoStats_Common()
    {
        var randomManagerMock = new Mock<IRandomManager>();
        var itemManagerMock = new Mock<IItemManager>();
        var roomMock = new Mock<IContainer>();

        var itemBlueprint = new ItemArmorBlueprint
        {
            Id = 98,
            Name = "breastplate",
            ShortDescription = "a breastplate",
            Description = "a breastplate is here",
            WearLocation = WearLocations.Chest,
            Level = 0,
            Cost = 0,
            NoTake = false,
            ItemFlags = new ItemFlags(),
            Bash = 0,
            Pierce = 0,
            Slash = 0,
            Exotic = 0,
            Weight = 5,
        };
        var itemMock = new Mock<IItem>();
        itemMock.SetupGet(x => x.Blueprint).Returns(itemBlueprint);

        randomManagerMock.Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((int min, int max) => 1); // 1 -> common
        randomManagerMock.Setup(x => x.Random(It.IsAny<IEnumerable<It.IsAnyType>>()))
            .Returns((IEnumerable list) => GetLast(list));
        itemManagerMock.Setup(x => x.AddItem(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IContainer>()))
            .Returns(itemMock.Object);

        var generator = new ItemGenerator(randomManagerMock.Object, itemManagerMock.Object);
        generator.AddBlueprintId(98);

        var item = generator.Generate(25, roomMock.Object);

        Assert.IsNotNull(item);
        // modifier = 25 * 1 * 1 / (2 * 4) = 3
        itemMock.Verify(x => x.AddAura(It.Is<IAura>(x =>
                x.Affects.Count() == 2 
                && x.Affects.OfType<CharacterAttributeAffect>().Count(x => x.Location == CharacterAttributeAffectLocations.Dexterity) == 1
                && x.Affects.OfType<CharacterAttributeAffect>().Count(x => x.Location == CharacterAttributeAffectLocations.Constitution) == 1
                && x.Affects.OfType<CharacterAttributeAffect>().All(x => x.Modifier == 3)),
            It.IsAny<bool>()), Times.Once);
        itemMock.Verify(x => x.SetLevel(25), Times.Once);
        itemMock.Verify(x => x.SetCost(10 * 25 * 100 / 100), Times.Once);
        itemMock.Verify(x => x.SetShortDescription("a breastplate of the monkey"), Times.Once);
        itemMock.Verify(x => x.SetDescription("A Breastplate of the Monkey is here."), Times.Once);
    }

    [TestMethod]
    public void TwoStats_Uncommon()
    {
        var randomManagerMock = new Mock<IRandomManager>();
        var itemManagerMock = new Mock<IItemManager>();
        var roomMock = new Mock<IContainer>();

        var itemBlueprint = new ItemArmorBlueprint
        {
            Id = 98,
            Name = "breastplate",
            ShortDescription = "a breastplate",
            Description = "a breastplate is here",
            WearLocation = WearLocations.Chest,
            Level = 0,
            Cost = 0,
            NoTake = false,
            ItemFlags = new ItemFlags(),
            Bash = 0,
            Pierce = 0,
            Slash = 0,
            Exotic = 0,
            Weight = 5,
        };
        var itemMock = new Mock<IItem>();
        itemMock.SetupGet(x => x.Blueprint).Returns(itemBlueprint);

        randomManagerMock.Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((int min, int max) => 75); // 75 -> uncommon
        randomManagerMock.Setup(x => x.Random(It.IsAny<IEnumerable<It.IsAnyType>>()))
            .Returns((IEnumerable list) => GetLast(list));
        itemManagerMock.Setup(x => x.AddItem(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IContainer>()))
            .Returns(itemMock.Object);

        var generator = new ItemGenerator(randomManagerMock.Object, itemManagerMock.Object);
        generator.AddBlueprintId(98);

        var item = generator.Generate(25, roomMock.Object);

        Assert.IsNotNull(item);
        // modifier = 25 * 1.5 * 1 / (2 * 4) = 4
        itemMock.Verify(x => x.AddAura(It.Is<IAura>(x =>
                x.Affects.Count() == 2
                && x.Affects.OfType<CharacterAttributeAffect>().Count(x => x.Location == CharacterAttributeAffectLocations.Dexterity) == 1
                && x.Affects.OfType<CharacterAttributeAffect>().Count(x => x.Location == CharacterAttributeAffectLocations.Constitution) == 1
                && x.Affects.OfType<CharacterAttributeAffect>().All(x => x.Modifier == 4)),
            It.IsAny<bool>()), Times.Once);
        itemMock.Verify(x => x.SetLevel(25), Times.Once);
        itemMock.Verify(x => x.SetCost(10 * 25 * 150 / 100), Times.Once);
        itemMock.Verify(x => x.SetShortDescription("a breastplate of the monkey"), Times.Once);
        itemMock.Verify(x => x.SetDescription("A Breastplate of the Monkey is here."), Times.Once);
    }

    [TestMethod]
    public void TwoStats_Rare()
    {
        var randomManagerMock = new Mock<IRandomManager>();
        var itemManagerMock = new Mock<IItemManager>();
        var roomMock = new Mock<IContainer>();

        var itemBlueprint = new ItemArmorBlueprint
        {
            Id = 98,
            Name = "breastplate",
            ShortDescription = "a breastplate",
            Description = "a breastplate is here",
            WearLocation = WearLocations.Chest,
            Level = 0,
            Cost = 0,
            NoTake = false,
            ItemFlags = new ItemFlags(),
            Bash = 0,
            Pierce = 0,
            Slash = 0,
            Exotic = 0,
            Weight = 5,
        };
        var itemMock = new Mock<IItem>();
        itemMock.SetupGet(x => x.Blueprint).Returns(itemBlueprint);

        randomManagerMock.Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((int min, int max) => 95); // 95 -> rare
        randomManagerMock.Setup(x => x.Random(It.IsAny<IEnumerable<It.IsAnyType>>()))
            .Returns((IEnumerable list) => GetLast(list));
        itemManagerMock.Setup(x => x.AddItem(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IContainer>()))
            .Returns(itemMock.Object);

        var generator = new ItemGenerator(randomManagerMock.Object, itemManagerMock.Object);
        generator.AddBlueprintId(98);

        var item = generator.Generate(25, roomMock.Object);

        Assert.IsNotNull(item);
        // modifier = 25 * 2 * 1 / (2 * 4) = 6
        itemMock.Verify(x => x.AddAura(It.Is<IAura>(x =>
                x.Affects.Count() == 2
                && x.Affects.OfType<CharacterAttributeAffect>().Count(x => x.Location == CharacterAttributeAffectLocations.Dexterity) == 1
                && x.Affects.OfType<CharacterAttributeAffect>().Count(x => x.Location == CharacterAttributeAffectLocations.Constitution) == 1
                && x.Affects.OfType<CharacterAttributeAffect>().All(x => x.Modifier == 6)),
            It.IsAny<bool>()), Times.Once);
        itemMock.Verify(x => x.SetLevel(25), Times.Once);
        itemMock.Verify(x => x.SetCost(10 * 25 * 200 / 100), Times.Once);
        itemMock.Verify(x => x.SetShortDescription("a breastplate of the monkey"), Times.Once);
        itemMock.Verify(x => x.SetDescription("A Breastplate of the Monkey is here."), Times.Once);
    }

    private object GetFirst(IEnumerable enumerable)
    {
        foreach (var item in enumerable)
            return item;
        return default!;
    }

    private object GetLast(IEnumerable enumerable)
    {
        object item = default!;
        foreach (var entry in enumerable)
            item = entry;
        return item;
    }
}
