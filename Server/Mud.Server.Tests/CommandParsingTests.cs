using Microsoft.Extensions.Logging;
using Moq;

namespace Mud.Server.Tests;

[TestClass]
public class CommandParsingTests : TestBase
{
    /*
    TODO: number/long argument
    TODO: aliases

    ICharacter character = _world.AddCharacter(Guid.NewGuid(), "Character", room);
    character.ProcessCommand("look");
    character.ProcessCommand("tell"); // INVALID because Player commands are not accessible by Character
    character.ProcessCommand("unknown"); // INVALID

    player.ProcessCommand("impersonate"); // INVALID to un-impersonate, player already must be impersonated
    player.ProcessCommand("impersonate character");
    player.ProcessCommand("/tell");
    player.ProcessCommand("tell"); // INVALID because OOG is not accessible while impersonating unless if command starts with /
    player.ProcessCommand("look");

    player.ProcessCommand("impersonate"); // INVALID because OOG is not accessible while impersonating unless if command starts with /
    player.ProcessCommand("/impersonate");
    player.ProcessCommand("/tell");
    player.ProcessCommand("tell");
    player.ProcessCommand("look"); // INVALID because Character commands are not accessible by Player unless if impersonating

    IAdmin admin = _world.AddAdmin(Guid.NewGuid(), "Admin");
    admin.ProcessCommand("incarnate");
    admin.ProcessCommand("unknown"); // INVALID
    */

    [TestMethod]
    public void NoArg()
    {
        var parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var parseResult = parser.Parse("test");

        Assert.IsNotNull(parseResult);
        Assert.AreEqual("test", parseResult.Command);
        Assert.IsEmpty(parseResult.Parameters);
    }

    [TestMethod]
    public void SingleArg()
    {
        var parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var parseResult = parser.Parse("test arg1");

        Assert.IsNotNull(parseResult);
        Assert.AreEqual("test", parseResult.Command);
        Assert.ContainsSingle(parseResult.Parameters);
        Assert.IsNotNull(parseResult.Parameters[0]);
        Assert.AreEqual("arg1", parseResult.Parameters[0].RawValue);
        Assert.AreEqual("arg1", parseResult.Parameters[0].Value);
        Assert.AreEqual(1, parseResult.Parameters[0].Count);
        Assert.IsFalse(parseResult.Parameters[0].IsNumber);
        Assert.IsFalse(parseResult.Parameters[0].IsLong);
        Assert.IsFalse(parseResult.Parameters[0].IsAll);
    }

    [TestMethod]
    public void SingleArg_2()
    {
        var parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var parseResult = parser.Parse("test 'arg1 arg2 arg3 arg4'");

        Assert.IsNotNull(parseResult);
        Assert.AreEqual("test", parseResult.Command);
        Assert.ContainsSingle(parseResult.Parameters);
        Assert.IsNotNull(parseResult.Parameters[0]);
        Assert.AreEqual("arg1 arg2 arg3 arg4", parseResult.Parameters[0].RawValue);
        Assert.AreEqual("arg1 arg2 arg3 arg4", parseResult.Parameters[0].Value);
        Assert.AreEqual(1, parseResult.Parameters[0].Count);
        Assert.IsFalse(parseResult.Parameters[0].IsNumber);
        Assert.IsFalse(parseResult.Parameters[0].IsLong);
        Assert.IsFalse(parseResult.Parameters[0].IsAll);
    }

    [TestMethod]
    public void MultipleArgs()
    {
        var parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var parseResult = parser.Parse("test 'arg1' 'arg2' 'arg3' 'arg4'");

        Assert.IsNotNull(parseResult);
        Assert.AreEqual("test", parseResult.Command);
        Assert.HasCount(4, parseResult.Parameters);
        Assert.AreEqual("arg1", parseResult.Parameters[0].RawValue);
        Assert.AreEqual("arg1", parseResult.Parameters[0].Value);
        Assert.AreEqual(1, parseResult.Parameters[0].Count);
        Assert.IsFalse(parseResult.Parameters[0].IsNumber);
        Assert.IsFalse(parseResult.Parameters[0].IsLong);
        Assert.IsFalse(parseResult.Parameters[0].IsAll);
        Assert.AreEqual("arg2", parseResult.Parameters[1].RawValue);
        Assert.AreEqual("arg2", parseResult.Parameters[1].Value);
        Assert.AreEqual(1, parseResult.Parameters[1].Count);
        Assert.IsFalse(parseResult.Parameters[1].IsNumber);
        Assert.IsFalse(parseResult.Parameters[1].IsLong);
        Assert.IsFalse(parseResult.Parameters[1].IsAll);
        Assert.AreEqual("arg3", parseResult.Parameters[2].RawValue);
        Assert.AreEqual("arg3", parseResult.Parameters[2].Value);
        Assert.AreEqual(1, parseResult.Parameters[2].Count);
        Assert.IsFalse(parseResult.Parameters[2].IsNumber);
        Assert.IsFalse(parseResult.Parameters[2].IsLong);
        Assert.IsFalse(parseResult.Parameters[2].IsAll);
        Assert.AreEqual("arg4", parseResult.Parameters[3].RawValue);
        Assert.AreEqual("arg4", parseResult.Parameters[3].Value);
        Assert.AreEqual(1, parseResult.Parameters[3].Count);
        Assert.IsFalse(parseResult.Parameters[3].IsNumber);
        Assert.IsFalse(parseResult.Parameters[3].IsLong);
        Assert.IsFalse(parseResult.Parameters[3].IsAll);
    }

    [TestMethod]
    public void MultipleArgs_2()
    {
        var parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var parseResult = parser.Parse("test 'arg1 arg2' 'arg3 arg4'");

        Assert.IsNotNull(parseResult);
        Assert.AreEqual("test", parseResult.Command);
        Assert.HasCount(2, parseResult.Parameters);
        Assert.AreEqual("arg1 arg2", parseResult.Parameters[0].RawValue);
        Assert.AreEqual("arg1 arg2", parseResult.Parameters[0].Value);
        Assert.AreEqual(1, parseResult.Parameters[0].Count);
        Assert.IsFalse(parseResult.Parameters[0].IsNumber);
        Assert.IsFalse(parseResult.Parameters[0].IsLong);
        Assert.IsFalse(parseResult.Parameters[0].IsAll);
        Assert.AreEqual("arg3 arg4", parseResult.Parameters[1].RawValue);
        Assert.AreEqual("arg3 arg4", parseResult.Parameters[1].Value);
        Assert.AreEqual(1, parseResult.Parameters[1].Count);
        Assert.IsFalse(parseResult.Parameters[1].IsNumber);
        Assert.IsFalse(parseResult.Parameters[1].IsLong);
        Assert.IsFalse(parseResult.Parameters[1].IsAll);
    }

    [TestMethod]
    public void MultipleArgs_3()
    {
        var parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var parseResult = parser.Parse("test 'arg1 arg2\" arg3 arg4");

        Assert.IsNotNull(parseResult);
        Assert.AreEqual("test", parseResult.Command);
        Assert.HasCount(3, parseResult.Parameters);
        Assert.AreEqual("arg1 arg2", parseResult.Parameters[0].RawValue);
        Assert.AreEqual("arg1 arg2", parseResult.Parameters[0].Value);
        Assert.AreEqual(1, parseResult.Parameters[0].Count);
        Assert.IsFalse(parseResult.Parameters[0].IsNumber);
        Assert.IsFalse(parseResult.Parameters[0].IsLong);
        Assert.IsFalse(parseResult.Parameters[0].IsAll);
        Assert.AreEqual("arg3", parseResult.Parameters[1].RawValue);
        Assert.AreEqual("arg3", parseResult.Parameters[1].Value);
        Assert.AreEqual(1, parseResult.Parameters[1].Count);
        Assert.IsFalse(parseResult.Parameters[1].IsNumber);
        Assert.IsFalse(parseResult.Parameters[1].IsLong);
        Assert.IsFalse(parseResult.Parameters[1].IsAll);
        Assert.AreEqual("arg4", parseResult.Parameters[2].RawValue);
        Assert.AreEqual("arg4", parseResult.Parameters[2].Value);
        Assert.AreEqual(1, parseResult.Parameters[2].Count);
        Assert.IsFalse(parseResult.Parameters[2].IsNumber);
        Assert.IsFalse(parseResult.Parameters[2].IsLong);
        Assert.IsFalse(parseResult.Parameters[2].IsAll);
    }

    [TestMethod]
    public void SingleArg_Count()
    {
        var parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var parseResult = parser.Parse("test 3.arg1");

        Assert.IsNotNull(parseResult);
        Assert.AreEqual("test", parseResult.Command);
        Assert.ContainsSingle(parseResult.Parameters);
        Assert.IsNotNull(parseResult.Parameters[0]);
        Assert.AreEqual("3.arg1", parseResult.Parameters[0].RawValue);
        Assert.AreEqual("arg1", parseResult.Parameters[0].Value);
        Assert.AreEqual(3, parseResult.Parameters[0].Count);
        Assert.IsFalse(parseResult.Parameters[0].IsNumber);
        Assert.IsFalse(parseResult.Parameters[0].IsLong);
        Assert.IsFalse(parseResult.Parameters[0].IsAll);
    }

    [TestMethod]
    public void SingleArg_Count_2()
    {
        var parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var parseResult = parser.Parse("test 2.'arg1'");

        Assert.IsNotNull(parseResult);
        Assert.AreEqual("test", parseResult.Command);
        Assert.ContainsSingle(parseResult.Parameters);
        Assert.IsNotNull(parseResult.Parameters[0]);
        Assert.AreEqual("2.arg1", parseResult.Parameters[0].RawValue);
        Assert.AreEqual("arg1", parseResult.Parameters[0].Value);
        Assert.AreEqual(2, parseResult.Parameters[0].Count);
        Assert.IsFalse(parseResult.Parameters[0].IsNumber);
        Assert.IsFalse(parseResult.Parameters[0].IsLong);
        Assert.IsFalse(parseResult.Parameters[0].IsAll);
    }

    [TestMethod]
    public void SingleArg_Count_3()
    {
        var parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var parseResult = parser.Parse("test 2'.arg1'");

        Assert.IsNotNull(parseResult);
        Assert.AreEqual("test", parseResult.Command);
        Assert.ContainsSingle(parseResult.Parameters);
        Assert.IsNotNull(parseResult.Parameters[0]);
        Assert.AreEqual("2.arg1", parseResult.Parameters[0].RawValue);
        Assert.AreEqual("arg1", parseResult.Parameters[0].Value);
        Assert.AreEqual(2, parseResult.Parameters[0].Count);
        Assert.IsFalse(parseResult.Parameters[0].IsNumber);
        Assert.IsFalse(parseResult.Parameters[0].IsLong);
        Assert.IsFalse(parseResult.Parameters[0].IsAll);
    }

    [TestMethod]
    public void MultipleArgs_Count()
    {
        var parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var parseResult = parser.Parse("test 2'.arg1' arg3");

        Assert.IsNotNull(parseResult);
        Assert.AreEqual("test", parseResult.Command);
        Assert.HasCount(2, parseResult.Parameters);
        Assert.AreEqual("2.arg1", parseResult.Parameters[0].RawValue);
        Assert.AreEqual("arg1", parseResult.Parameters[0].Value);
        Assert.AreEqual(2, parseResult.Parameters[0].Count);
        Assert.IsFalse(parseResult.Parameters[0].IsNumber);
        Assert.IsFalse(parseResult.Parameters[0].IsLong);
        Assert.IsFalse(parseResult.Parameters[0].IsAll);
        Assert.AreEqual("arg3", parseResult.Parameters[1].RawValue);
        Assert.AreEqual("arg3", parseResult.Parameters[1].Value);
        Assert.AreEqual(1, parseResult.Parameters[1].Count);
        Assert.IsFalse(parseResult.Parameters[1].IsNumber);
        Assert.IsFalse(parseResult.Parameters[1].IsLong);
        Assert.IsFalse(parseResult.Parameters[1].IsAll);
    }

    [TestMethod]
    public void MultipleArgs_Count_2()
    {
        var parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var parseResult = parser.Parse("test 2.'arg1 arg2' 3.arg3 5.arg4");

        Assert.IsNotNull(parseResult);
        Assert.AreEqual("test", parseResult.Command);
        Assert.HasCount(3, parseResult.Parameters);
        Assert.AreEqual("2.arg1 arg2", parseResult.Parameters[0].RawValue);
        Assert.AreEqual("arg1 arg2", parseResult.Parameters[0].Value);
        Assert.AreEqual(2, parseResult.Parameters[0].Count);
        Assert.IsFalse(parseResult.Parameters[0].IsNumber);
        Assert.IsFalse(parseResult.Parameters[0].IsLong);
        Assert.IsFalse(parseResult.Parameters[0].IsAll);
        Assert.AreEqual("3.arg3", parseResult.Parameters[1].RawValue);
        Assert.AreEqual("arg3", parseResult.Parameters[1].Value);
        Assert.AreEqual(3, parseResult.Parameters[1].Count);
        Assert.IsFalse(parseResult.Parameters[1].IsNumber);
        Assert.IsFalse(parseResult.Parameters[1].IsLong);
        Assert.IsFalse(parseResult.Parameters[1].IsAll);
        Assert.AreEqual("5.arg4", parseResult.Parameters[2].RawValue);
        Assert.AreEqual("arg4", parseResult.Parameters[2].Value);
        Assert.AreEqual(5, parseResult.Parameters[2].Count);
        Assert.IsFalse(parseResult.Parameters[2].IsNumber);
        Assert.IsFalse(parseResult.Parameters[2].IsLong);
        Assert.IsFalse(parseResult.Parameters[2].IsAll);
    }

    [TestMethod]
    public void SingleArg_Count_Invalid()
    {
        var parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var parseResult = parser.Parse("test 2.");

        Assert.IsNull(parseResult);
    }

    [TestMethod]
    public void SingleArg_Count_Invalid_2()
    {
        var parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var parseResult = parser.Parse("test .");

        Assert.IsNull(parseResult);
    }

    [TestMethod]
    public void SingleArg_Count_4()
    {
        var parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var parseResult = parser.Parse("test '2.arg1'");

        Assert.IsNotNull(parseResult);
        Assert.AreEqual("test", parseResult.Command);
        Assert.ContainsSingle(parseResult.Parameters);
        Assert.IsNotNull(parseResult.Parameters[0]);
        Assert.AreEqual("2.arg1", parseResult.Parameters[0].RawValue);
        Assert.AreEqual("arg1", parseResult.Parameters[0].Value);
        Assert.AreEqual(2, parseResult.Parameters[0].Count);
        Assert.IsFalse(parseResult.Parameters[0].IsNumber);
        Assert.IsFalse(parseResult.Parameters[0].IsLong);
        Assert.IsFalse(parseResult.Parameters[0].IsAll);
    }

    [TestMethod]
    public void NoArg_OutOfGame()
    {
        var parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var parseResult = parser.Parse("/test");

        Assert.IsNotNull(parseResult);
        Assert.AreEqual("test", parseResult.Command);
        Assert.IsEmpty(parseResult.Parameters);
    }

    [TestMethod]
    public void NoArg_DetectOutOfGame()
    {
        var parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var parseResult = parser.Parse(null, "/test");

        Assert.IsNotNull(parseResult);
        Assert.AreEqual("test", parseResult.Command);
        Assert.IsEmpty(parseResult.Parameters);
        Assert.IsTrue(parseResult.ForceOutOfGame);
    }

    [TestMethod]
    public void NoArg_DetectOutOfGame_2()
    {
        var parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var parseResult = parser.Parse(null, "test");

        Assert.IsNotNull(parseResult);
        Assert.AreEqual("test", parseResult.Command);
        Assert.IsEmpty(parseResult.Parameters);
        Assert.IsFalse(parseResult.ForceOutOfGame);
    }

    [TestMethod]
    public void CommaCommandNoSpace()
    {
        var parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var parseResult = parser.Parse(null, "'toto");
        Assert.IsNotNull(parseResult);
        Assert.AreEqual("'", parseResult.Command);
        Assert.IsNotEmpty(parseResult.Parameters);
        Assert.AreEqual("toto", parseResult.Parameters[0].Value);
    }

    [TestMethod]
    public void CommaCommand()
    {
        var parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var parseResult = parser.Parse(null, "' toto");
        Assert.IsNotNull(parseResult);
        Assert.AreEqual("'", parseResult.Command);
        Assert.IsNotEmpty(parseResult.Parameters);
        Assert.AreEqual("toto", parseResult.Parameters[0].Value);
    }


    [TestMethod]
    public void CommaCommandWithCommaInParameters()
    {
        var parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var parseResult = parser.Parse(null, "' 'toto'");
        Assert.IsNotNull(parseResult);
        Assert.AreEqual("'", parseResult.Command);
        Assert.IsNotEmpty(parseResult.Parameters);
        Assert.AreEqual("toto", parseResult.Parameters[0].Value);
    }
}
