namespace Mud.DataStructures.Tests;

[TestClass]
public class ArrayByEnumTests
{
    [TestMethod]
    public void Ctor_NoGap()
    {
        ArrayByEnum<int, TestEnum_NoGap> arrayByEnum = new();

        Assert.IsNotNull(arrayByEnum);
        Assert.HasCount(3, arrayByEnum);
    }

    [TestMethod]
    public void SetAtIndex_NoGap()
    {
        ArrayByEnum<int, TestEnum_NoGap> arrayByEnum = new();

        arrayByEnum[TestEnum_NoGap.First] = 10;

        Assert.IsNotNull(arrayByEnum);
        Assert.HasCount(3, arrayByEnum);
        Assert.Contains(10, arrayByEnum);
    }

    [TestMethod]
    public void GetAtIndex_NoGap()
    {
        ArrayByEnum<int, TestEnum_NoGap> arrayByEnum = new();
        arrayByEnum[TestEnum_NoGap.First] = 10;
        arrayByEnum[TestEnum_NoGap.Second] = 0;
        arrayByEnum[TestEnum_NoGap.Third] = -17;

        Assert.IsNotNull(arrayByEnum);
        Assert.HasCount(3, arrayByEnum);
        Assert.AreEqual(10, arrayByEnum[TestEnum_NoGap.First]);
        Assert.AreEqual(0, arrayByEnum[TestEnum_NoGap.Second]);
        Assert.AreEqual(-17, arrayByEnum[TestEnum_NoGap.Third]);
    }

    [TestMethod]
    public void Ctor_WithGaps()
    {
        ArrayByEnum<int, TestEnumWithGaps> arrayByEnum = new();

        Assert.IsNotNull(arrayByEnum);
        Assert.HasCount(3, arrayByEnum);
    }

    [TestMethod]
    public void SetAtIndex_WithGaps()
    {
        ArrayByEnum<int, TestEnumWithGaps> arrayByEnum = new();

        arrayByEnum[TestEnumWithGaps.First] = 10;

        Assert.IsNotNull(arrayByEnum);
        Assert.HasCount(3, arrayByEnum);
        Assert.Contains(10, arrayByEnum);
    }

    [TestMethod]
    public void GetAtIndex_WithGaps()
    {
        ArrayByEnum<int, TestEnumWithGaps> arrayByEnum = new();
        arrayByEnum[TestEnumWithGaps.First] = 10;
        arrayByEnum[TestEnumWithGaps.Second] = 0;
        arrayByEnum[TestEnumWithGaps.Third] = -17;

        Assert.IsNotNull(arrayByEnum);
        Assert.HasCount(3, arrayByEnum);
        Assert.AreEqual(10, arrayByEnum[TestEnumWithGaps.First]);
        Assert.AreEqual(0, arrayByEnum[TestEnumWithGaps.Second]);
        Assert.AreEqual(-17, arrayByEnum[TestEnumWithGaps.Third]);
    }

    public enum TestEnum_NoGap
    {
        First = 0,
        Second = 1,
        Third = 2
    }

    public enum TestEnumWithGaps
    {
        First = -7,
        Second = 0,
        Third = 4
    }
}
