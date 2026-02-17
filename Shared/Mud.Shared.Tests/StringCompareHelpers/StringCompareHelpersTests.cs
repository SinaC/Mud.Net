namespace Mud.Shared.Tests.StringCompareHelpers;

[TestClass]
public class StringCompareHelpersTests
{
    [TestMethod]
    [DataRow(new string[] { "oldstyle" }, new string[] { "jailer" }, false)]
    [DataRow(new string[] { "jailer" }, new string[] { "jailer" }, true)]
    [DataRow(new string[] { "jailer" }, new string[] { "oldstyle", "jailer" }, false)]
    [DataRow(new string[] { "oldstyle", "jailer" }, new string[] { "jailer" }, true)]
    [DataRow(new string[] { "oldstyle", "jailer" }, new string[] { "oldstyle" }, true)]
    [DataRow(new string[] { "oldstyle", "jailer" }, new string[] { "jailer", "oldstyle" }, true)]
    public void AllStringsEquals(string[] keys, string[] parameters, bool expected)
    {
        var result = Common.StringCompareHelpers.AllStringsEquals(keys, parameters);

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    [DataRow(new string[] { "oldstyle" }, new string[] { "jailer" }, false)]
    [DataRow(new string[] { "jailer" }, new string[] { "jailer" }, true)]
    [DataRow(new string[] { "jailer" }, new string[] { "oldstyle", "jailer" }, false)]
    [DataRow(new string[] { "oldstyle", "jailer" }, new string[] { "jailer" }, true)]
    [DataRow(new string[] { "oldstyle", "jailer" }, new string[] { "oldstyle" }, true)]
    [DataRow(new string[] { "oldstyle", "jailer" }, new string[] { "jailer", "oldstyle" }, true)]
    [DataRow(new string[] { "oldstyle" }, new string[] { "o" }, true)]
    [DataRow(new string[] { "j" }, new string[] { "jailer" }, false)]
    [DataRow(new string[] { "oldstyle", "jailer" }, new string[] { "j" }, true)]
    [DataRow(new string[] { "oldstyle", "jailer" }, new string[] { "old" }, true)]
    public void AllStringsStartsWith(string[] keys, string[] parameters, bool expected)
    {
        var result = Common.StringCompareHelpers.AllStringsStartsWith(keys, parameters);

        Assert.AreEqual(expected, result);
    }
}
