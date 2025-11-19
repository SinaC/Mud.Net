using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Mud.DataStructures.Flags;

namespace Mud.DataStructures.Tests;

[TestClass]
public class FlagsWithValuesTests
{
    [TestMethod]
    public void Ctor()
    {
        var flags = new CharacterFlags(_serviceProvider);

        Assert.AreEqual(0, flags.Count);
    }

    [TestMethod]
    public void Ctor_MultipleValues_Valid()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind", "Charm", "Berserk");

        Assert.AreEqual(3, flags.Count);
    }

    [TestMethod]
    public void Ctor_MultipleValues_Invalid()
    {
        Assert.Throws<ArgumentException>(() => new CharacterFlags(_serviceProvider, "Blind", "Invalid", "Berserk"));

    }

    [TestMethod]
    public void HasAny_SameFlagTypes()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind", "Charm", "Berserk");
        var flags2 = new CharacterFlags(_serviceProvider, "Curse", "Charm", "Berserk");

        bool hasAny = flags.HasAny(flags2);

        Assert.IsTrue(hasAny);
    }

    // Doesn't compile
    //[TestMethod]
    //public void HasAny_DifferentFlagTypes()
    //{
    //    Flags<ICharacterFlags> flags = new Flags<ICharacterFlags>("Blind", "Charm", "Berserk");
    //    Flags<IRoomFlags> flags2 = new Flags<IRoomFlags>("Dark", "NoMob", "Berserk");

    //    bool hasAny = flags.HasAny(flags2);
    //}

    [TestMethod]
    public void HasAll_SameFlagTypes()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind", "Charm", "Berserk");
        var flags2 = new CharacterFlags(_serviceProvider, "Curse", "Charm", "Berserk");

        bool hasAll = flags.HasAll(flags2);

        Assert.IsFalse(hasAll);
    }


    #region Set

    [TestMethod]
    public void Set_OneFlag_Count()
    {
        var flags = new CharacterFlags(_serviceProvider);

        flags.Set("Blind");

        Assert.AreEqual(1, flags.Count);
    }

    [TestMethod]
    public void Set_OneFlag_IsSet()
    {
        var flags = new CharacterFlags(_serviceProvider);

        flags.Set("Blind");

        Assert.IsTrue(flags.IsSet("Blind"));
    }

    [TestMethod]
    public void Set_OneFlag_IsSetMixedCase()
    {
        var flags = new CharacterFlags(_serviceProvider);

        flags.Set("Blind");

        Assert.IsTrue(flags.IsSet("Blind"));
    }

    [TestMethod]
    public void Set_OneFlag_CheckAnotherFlag()
    {
        var flags = new CharacterFlags(_serviceProvider);

        flags.Set("Blind");

        Assert.IsFalse(flags.IsSet("Charm"));
    }

    [TestMethod]
    public void Set_TwoFlags_Count()
    {
        var flags = new CharacterFlags(_serviceProvider);

        flags.Set("Blind");
        flags.Set("Charm");

        Assert.AreEqual(2, flags.Count);
    }

    [TestMethod]
    public void Set_TwoFlags_Set()
    {
        var flags = new CharacterFlags(_serviceProvider);

        flags.Set("Blind");
        flags.Set("Charm");

        Assert.IsTrue(flags.IsSet("Blind"));
        Assert.IsTrue(flags.IsSet("Charm"));
    }

    [TestMethod]
    public void Set_MultipleMixedCaseFlags_Set()
    {
        var flags = new CharacterFlags(_serviceProvider);

        flags.Set("BLind");
        flags.Set("BlInd");
        flags.Set("BliNd");
        flags.Set("BLIND");

        Assert.IsTrue(flags.IsSet("Blind"));
    }

    [TestMethod]
    public void Set_MultipleMixedCaseFlags_Count()
    {
        var flags = new CharacterFlags(_serviceProvider);

        flags.Set("BLind");
        flags.Set("BlInd");
        flags.Set("BliNd");
        flags.Set("BLIND");

        Assert.AreEqual(1, flags.Count);
    }

    #endregion

    #region Set params

    [TestMethod]
    public void SetParams_Multiple_Count()
    {
        var flags = new CharacterFlags(_serviceProvider);

        flags.Set("Blind", "Charm");

        Assert.AreEqual(2, flags.Count);
    }

    [TestMethod]
    public void SetParams_Multiple_IsSet()
    {
        var flags = new CharacterFlags(_serviceProvider);

        flags.Set("Blind", "Charm");

        Assert.IsTrue(flags.IsSet("Blind"));
        Assert.IsTrue(flags.IsSet("Charm"));
    }

    [TestMethod]
    public void SetParams_MultipleMixedCase_Count()
    {
        var flags = new CharacterFlags(_serviceProvider);

        flags.Set("Blind", "Blind");

        Assert.AreEqual(1, flags.Count);
    }

    [TestMethod]
    public void SetParams_MultipleMixedCase_IsSet()
    {
        var flags = new CharacterFlags(_serviceProvider);

        flags.Set("Blind", "Charm");

        Assert.IsTrue(flags.IsSet("Blind"));
    }

    #endregion

    #region Set flags

    [TestMethod]
    public void SetFlags_DifferentFlags_Count()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind", "Charm", "Hide");
        var flags2 = new CharacterFlags(_serviceProvider, "Berserk", "Slow", "Sneak");

        flags.Set(flags2);

        Assert.AreEqual(6, flags.Count);
        Assert.AreEqual(3, flags2.Count);
    }

    [TestMethod]
    public void SetFlags_IdenticalFlags_Count()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind", "Charm", "Hide");
        var flags2 = new CharacterFlags(_serviceProvider, "Blind", "Charm", "Hide");

        flags.Set(flags2);

        Assert.AreEqual(3, flags.Count);
        Assert.AreEqual(3, flags2.Count);
    }

    [TestMethod]
    public void SetFlags_IdenticalAndDifferentFlags_Count()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind", "Charm", "Hide");
        var flags2 = new CharacterFlags(_serviceProvider, "Hide", "Berserk", "Slow");

        flags.Set(flags2);

        Assert.AreEqual(5, flags.Count);
        Assert.AreEqual(3, flags2.Count);
    }

    #endregion

    #region Unset

    [TestMethod]
    public void Unset_ExistingFlag_Count()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind");

        flags.Unset("Blind");

        Assert.AreEqual(0, flags.Count);
    }

    [TestMethod]
    public void Unset_ExistingFlag_IsSet()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind");

        flags.Unset("Blind");

        Assert.IsFalse(flags.IsSet("Blind"));
    }

    [TestMethod]
    public void Unset_InexistingFlag_Count()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind");

        flags.Unset("Charm");

        Assert.AreEqual(1, flags.Count);
    }

    [TestMethod]
    public void Unset_InexistingFlag_IsSet()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind");

        flags.Unset("Charm");

        Assert.IsTrue(flags.IsSet("Blind"));
        Assert.IsFalse(flags.IsSet("Charm"));
    }

    #endregion

    #region Unset params

    [TestMethod]
    public void UnsetParams_MultipleExisting_Count()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind", "Charm", "Hide");

        flags.Unset("Blind", "Hide");

        Assert.AreEqual(1, flags.Count);
    }

    [TestMethod]
    public void UnsetParams_MultipleExisting_IsSet()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind", "Charm", "Hide");

        flags.Unset("Blind", "Hide");

        Assert.IsFalse(flags.IsSet("Blind"));
        Assert.IsTrue(flags.IsSet("Charm"));
        Assert.IsFalse(flags.IsSet("Hide"));
    }

    [TestMethod]
    public void UnsetParams_MultipleExistingAndInexisting_Count()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind", "Charm", "Hide");

        flags.Unset("Blind", "Berserk");

        Assert.AreEqual(2, flags.Count);
    }

    [TestMethod]
    public void UnsetParams_MultipleExistingAndInexisting_IsSet()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind", "Charm", "Hide");

        flags.Unset("Blind", "Berserk");

        Assert.IsFalse(flags.IsSet("Blind"));
        Assert.IsTrue(flags.IsSet("Charm"));
        Assert.IsTrue(flags.IsSet("Hide"));
    }

    #endregion

    #region HasAny

    [TestMethod]
    public void HasAny_Existing()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind", "Charm", "Hide");

        bool hasAny = flags.HasAny("Blind", "Hide");

        Assert.IsTrue(hasAny);
    }

    [TestMethod]
    public void HasAny_Inexisting()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind", "Charm", "Hide");

        bool hasAny = flags.HasAny("Slow", "Berserk");

        Assert.IsFalse(hasAny);
    }

    [TestMethod]
    public void HasAny_MultipleExistingAndInexisting()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind", "Charm", "Hide");

        bool hasAny = flags.HasAny("Blind", "Berserk");

        Assert.IsTrue(hasAny);
    }

    #endregion

    #region HasAny Flag

    [TestMethod]
    public void HasAnyFlag_Existing()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind", "Charm", "Hide");
        var flags2 = new CharacterFlags(_serviceProvider, "Blind", "Hide");

        bool hasAny = flags.HasAny(flags2);

        Assert.IsTrue(hasAny);
    }

    [TestMethod]
    public void HasAnyFlag_Inexisting()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind", "Charm", "Hide");
        var flags2 = new CharacterFlags(_serviceProvider, "Slow", "Berserk");

        bool hasAny = flags.HasAny(flags2);

        Assert.IsFalse(hasAny);
    }

    [TestMethod]
    public void HasAnyFlag_MultipleExistingAndInexisting()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind", "Charm", "Hide");
        var flags2 = new CharacterFlags(_serviceProvider, "Blind", "Berserk");

        bool hasAny = flags.HasAny(flags2);

        Assert.IsTrue(hasAny);
    }

    #endregion

    #region HasAll

    [TestMethod]
    public void HasAll_Existing()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind", "Charm", "Hide");

        bool hasAll = flags.HasAll("Blind", "Hide");

        Assert.IsTrue(hasAll);
    }

    [TestMethod]
    public void HasAll_Inexisting()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind", "Charm", "Hide");

        bool hasAll = flags.HasAll("Slow", "Berserk");

        Assert.IsFalse(hasAll);
    }

    [TestMethod]
    public void HasAll_MultipleExistingAndInexisting()
    {
        Flags<ICharacterFlagValues> flags = new CharacterFlags(_serviceProvider, "Blind", "Charm", "Hide");

        bool hasAll = flags.HasAll("Blind", "Berserk");

        Assert.IsFalse(hasAll);
    }

    #endregion

    #region HasAll Flag

    [TestMethod]
    public void HasAllFlag_Existing()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind", "Charm", "Hide");
        var flags2 = new CharacterFlags(_serviceProvider, "Blind", "Hide");

        bool hasAll = flags.HasAll(flags2);

        Assert.IsTrue(hasAll);
    }

    [TestMethod]
    public void HasAllFlag_Inexisting()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind", "Charm", "Hide");
        var flags2 = new CharacterFlags(_serviceProvider, "Slow", "Berserk");

        bool hasAll = flags.HasAll(flags2);

        Assert.IsFalse(hasAll);
    }

    [TestMethod]
    public void HasAllFlag_MultipleExistingAndInexisting()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind", "Charm", "Hide");
        var flags2 = new CharacterFlags(_serviceProvider, "Blind", "Berserk");

        bool hasAll = flags.HasAll(flags2);

        Assert.IsFalse(hasAll);
    }

    #endregion

    #region Items

    [TestMethod]
    public void Items_NoFlag()
    {
        var flags = new CharacterFlags(_serviceProvider);

        IEnumerable<string> items = flags.Items;

        Assert.AreEqual(flags.Count, items.Count());
    }

    [TestMethod]
    public void Items_OneFlag()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind");

        IEnumerable<string> items = flags.Items;

        Assert.AreEqual(flags.Count, items.Count());
        Assert.AreEqual("Blind", items.First());
    }

    [TestMethod]
    public void Items_MultipleFlag()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind", "Charm", "Hide", "Berserk");

        IEnumerable<string> items = flags.Items;

        Assert.AreEqual(flags.Count, items.Count());
        Assert.AreEqual(1, items.Count(x => x == "Blind"));
        Assert.AreEqual(1, items.Count(x => x == "Charm"));
        Assert.AreEqual(1, items.Count(x => x == "Hide"));
        Assert.AreEqual(1, items.Count(x => x == "Berserk"));
    }

    #endregion

    #region Ctor parse

    [TestMethod]
    public void CtorParse_Null()
    {
        var flags = new CharacterFlags(_serviceProvider, (string)null!);

        Assert.IsNotNull(flags);
        Assert.AreEqual(0, flags.Count);
    }

    [TestMethod]
    public void CtorParse_Empty()
    {
        var flags = new CharacterFlags(_serviceProvider, string.Empty);

        Assert.IsNotNull(flags);
        Assert.AreEqual(0, flags.Count);
    }

    [TestMethod]
    public void CtorParse_OneValue()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind");

        Assert.IsNotNull(flags);
        Assert.AreEqual(1, flags.Count);
        Assert.IsTrue(flags.IsSet("Blind"));
    }

    [TestMethod]
    public void CtorParse_MultipleValues()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind,Charm,Hide");

        Assert.IsNotNull(flags);
        Assert.AreEqual(3, flags.Count);
        Assert.IsTrue(flags.IsSet("Blind"));
        Assert.IsTrue(flags.IsSet("Charm"));
        Assert.IsTrue(flags.IsSet("Hide"));
    }

    #endregion

    #region Map

    [TestMethod]
    public void Map_NoValue()
    {
        var flags = new CharacterFlags(_serviceProvider);

        Assert.AreEqual(string.Empty, flags.Map());
    }


    [TestMethod]
    public void Map_MultipleValues()
    {
        var flags = new CharacterFlags(_serviceProvider, "Blind", "Charm", "Hide");

        Assert.AreEqual("Blind,Charm,Hide", flags.Map());
    }

    #endregion

    #region ICharacterFlags

    [TestMethod]
    public void HasAny_ICharacterFlags()
    {
        var flags1 = new CharacterFlags(_serviceProvider, "Calm", "Berserk", "Blind");
        var flags2 = new CharacterFlags(_serviceProvider, "Sanctuary", "Blind", "Invisible");

        bool hasAny = flags1.HasAny(flags2);

        Assert.IsTrue(hasAny);
    }

    // Doesn't compile
    //[TestMethod]
    //public void HasAny_ICharacterFlags_IRoomFlags()
    //{
    //    var flags1 = new CharacterFlags("Calm", "Berserk", "Test");
    //    IRoomFlags flags2 = new RoomFlags("Dark", "NoMob", "Test");

    //    bool hasAny = flags1.HasAny(flags2);

    //    Assert.IsTrue(hasAny);
    //}

    #endregion

    #region TestInitialize/TestCleanup

    private IServiceProvider _serviceProvider = default!;

    [TestInitialize]
    public void TestInitialize()
    {
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(ICharacterFlagValues))) // don't mock IServiceProvider.GetRequiredService because it's an extension method
            .Returns(new Rom24CharacterFlags(new Mock<ILogger<Rom24CharacterFlags>>().Object));
        _serviceProvider = serviceProviderMock.Object;
    }

    #endregion
}

internal class CharacterFlags : Flags<ICharacterFlagValues>, ICharacterFlags
{
    public CharacterFlags(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    public CharacterFlags(IServiceProvider serviceProvider, string flags)
        : base(serviceProvider, flags)
    {
    }

    public CharacterFlags(IServiceProvider serviceProvider, params string[] flags)
        : base(serviceProvider, flags)
    {
    }
}

public interface ICharacterFlags : IFlags<string, ICharacterFlagValues>
{
}

public interface ICharacterFlagValues : IFlagValues<string>
{
}

public class Rom24CharacterFlags : FlagValuesBase<string>, ICharacterFlagValues
{
    public static readonly HashSet<string> Flags = new(StringComparer.InvariantCultureIgnoreCase)
    {
        "Blind",
        "Invisible",
        "DetectEvil",
        "DetectInvis",
        "DetectMagic",
        "DetectHidden",
        "DetectGood",
        "Sanctuary",
        "FaerieFire",
        "Infrared",
        "Curse",
        "Poison",
        "ProtectEvil",
        "ProtectGood",
        "Sneak",
        "Hide",
        "Sleep",
        "Charm",
        "Flying",
        "PassDoor",
        "Haste",
        "Calm",
        "Plague",
        "Weaken",
        "DarkVision",
        "Berserk",
        "Swim",
        "Regeneration",
        "Slow",
        "Test", // TEST PURPOSE
    };

    protected override HashSet<string> HashSet => Flags;

    public Rom24CharacterFlags(ILogger<Rom24CharacterFlags> logger)
        : base(logger)
    {
        
    }
}
