using Microsoft.Extensions.Logging;
using Moq;
using Mud.Server.GameAction;

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
        var Parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var processed = Parser.ExtractCommandAndParameters("test", out var command, out var parameters);

        Assert.IsTrue(processed);
        Assert.AreEqual("test", command);
        Assert.IsEmpty(parameters);
    }

    [TestMethod]
    public void SingleArg()
    {
        var Parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var processed = Parser.ExtractCommandAndParameters("test arg1", out var command, out var parameters);

        Assert.IsTrue(processed);
        Assert.AreEqual("test", command);
        Assert.ContainsSingle(parameters);
        Assert.IsNotNull(parameters[0]);
        Assert.AreEqual("arg1", parameters[0].RawValue);
        Assert.AreEqual("arg1", parameters[0].Value);
        Assert.AreEqual(1, parameters[0].Count);
        Assert.IsFalse(parameters[0].IsNumber);
        Assert.IsFalse(parameters[0].IsLong);
        Assert.IsFalse(parameters[0].IsAll);
    }

    [TestMethod]
    public void SingleArg_2()
    {
        var Parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var processed = Parser.ExtractCommandAndParameters("test 'arg1 arg2 arg3 arg4'", out var command, out var parameters);

        Assert.IsTrue(processed);
        Assert.AreEqual("test", command);
        Assert.ContainsSingle(parameters);
        Assert.IsNotNull(parameters[0]);
        Assert.AreEqual("arg1 arg2 arg3 arg4", parameters[0].RawValue);
        Assert.AreEqual("arg1 arg2 arg3 arg4", parameters[0].Value);
        Assert.AreEqual(1, parameters[0].Count);
        Assert.IsFalse(parameters[0].IsNumber);
        Assert.IsFalse(parameters[0].IsLong);
        Assert.IsFalse(parameters[0].IsAll);
    }

    [TestMethod]
    public void MultipleArgs()
    {
        var Parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var processed = Parser.ExtractCommandAndParameters("test 'arg1' 'arg2' 'arg3' 'arg4'", out var command, out var parameters);

        Assert.IsTrue(processed);
        Assert.AreEqual("test", command);
        Assert.HasCount(4, parameters);
        Assert.AreEqual("arg1", parameters[0].RawValue);
        Assert.AreEqual("arg1", parameters[0].Value);
        Assert.AreEqual(1, parameters[0].Count);
        Assert.IsFalse(parameters[0].IsNumber);
        Assert.IsFalse(parameters[0].IsLong);
        Assert.IsFalse(parameters[0].IsAll);
        Assert.AreEqual("arg2", parameters[1].RawValue);
        Assert.AreEqual("arg2", parameters[1].Value);
        Assert.AreEqual(1, parameters[1].Count);
        Assert.IsFalse(parameters[1].IsNumber);
        Assert.IsFalse(parameters[1].IsLong);
        Assert.IsFalse(parameters[1].IsAll);
        Assert.AreEqual("arg3", parameters[2].RawValue);
        Assert.AreEqual("arg3", parameters[2].Value);
        Assert.AreEqual(1, parameters[2].Count);
        Assert.IsFalse(parameters[2].IsNumber);
        Assert.IsFalse(parameters[2].IsLong);
        Assert.IsFalse(parameters[2].IsAll);
        Assert.AreEqual("arg4", parameters[3].RawValue);
        Assert.AreEqual("arg4", parameters[3].Value);
        Assert.AreEqual(1, parameters[3].Count);
        Assert.IsFalse(parameters[3].IsNumber);
        Assert.IsFalse(parameters[3].IsLong);
        Assert.IsFalse(parameters[3].IsAll);
    }

    [TestMethod]
    public void MultipleArgs_2()
    {
        var Parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var processed = Parser.ExtractCommandAndParameters("test 'arg1 arg2' 'arg3 arg4'", out var command, out var parameters);

        Assert.IsTrue(processed);
        Assert.AreEqual("test", command);
        Assert.HasCount(2, parameters);
        Assert.AreEqual("arg1 arg2", parameters[0].RawValue);
        Assert.AreEqual("arg1 arg2", parameters[0].Value);
        Assert.AreEqual(1, parameters[0].Count);
        Assert.IsFalse(parameters[0].IsNumber);
        Assert.IsFalse(parameters[0].IsLong);
        Assert.IsFalse(parameters[0].IsAll);
        Assert.AreEqual("arg3 arg4", parameters[1].RawValue);
        Assert.AreEqual("arg3 arg4", parameters[1].Value);
        Assert.AreEqual(1, parameters[1].Count);
        Assert.IsFalse(parameters[1].IsNumber);
        Assert.IsFalse(parameters[1].IsLong);
        Assert.IsFalse(parameters[1].IsAll);
    }

    [TestMethod]
    public void MultipleArgs_3()
    {
        var Parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var processed = Parser.ExtractCommandAndParameters("test 'arg1 arg2\" arg3 arg4", out var command, out var parameters);

        Assert.IsTrue(processed);
        Assert.AreEqual("test", command);
        Assert.HasCount(3, parameters);
        Assert.AreEqual("arg1 arg2", parameters[0].RawValue);
        Assert.AreEqual("arg1 arg2", parameters[0].Value);
        Assert.AreEqual(1, parameters[0].Count);
        Assert.IsFalse(parameters[0].IsNumber);
        Assert.IsFalse(parameters[0].IsLong);
        Assert.IsFalse(parameters[0].IsAll);
        Assert.AreEqual("arg3", parameters[1].RawValue);
        Assert.AreEqual("arg3", parameters[1].Value);
        Assert.AreEqual(1, parameters[1].Count);
        Assert.IsFalse(parameters[1].IsNumber);
        Assert.IsFalse(parameters[1].IsLong);
        Assert.IsFalse(parameters[1].IsAll);
        Assert.AreEqual("arg4", parameters[2].RawValue);
        Assert.AreEqual("arg4", parameters[2].Value);
        Assert.AreEqual(1, parameters[2].Count);
        Assert.IsFalse(parameters[2].IsNumber);
        Assert.IsFalse(parameters[2].IsLong);
        Assert.IsFalse(parameters[2].IsAll);
    }

    [TestMethod]
    public void SingleArg_Count()
    {
        var Parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var processed = Parser.ExtractCommandAndParameters("test 3.arg1", out var command, out var parameters);

        Assert.IsTrue(processed);
        Assert.AreEqual("test", command);
        Assert.ContainsSingle(parameters);
        Assert.IsNotNull(parameters[0]);
        Assert.AreEqual("3.arg1", parameters[0].RawValue);
        Assert.AreEqual("arg1", parameters[0].Value);
        Assert.AreEqual(3, parameters[0].Count);
        Assert.IsFalse(parameters[0].IsNumber);
        Assert.IsFalse(parameters[0].IsLong);
        Assert.IsFalse(parameters[0].IsAll);
    }

    [TestMethod]
    public void SingleArg_Count_2()
    {
        var Parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var processed = Parser.ExtractCommandAndParameters("test 2.'arg1'", out var command, out var parameters);

        Assert.IsTrue(processed);
        Assert.AreEqual("test", command);
        Assert.ContainsSingle(parameters);
        Assert.IsNotNull(parameters[0]);
        Assert.AreEqual("2.arg1", parameters[0].RawValue);
        Assert.AreEqual("arg1", parameters[0].Value);
        Assert.AreEqual(2, parameters[0].Count);
        Assert.IsFalse(parameters[0].IsNumber);
        Assert.IsFalse(parameters[0].IsLong);
        Assert.IsFalse(parameters[0].IsAll);
    }

    [TestMethod]
    public void SingleArg_Count_3()
    {
        var Parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var processed = Parser.ExtractCommandAndParameters("test 2'.arg1'", out var command, out var parameters);

        Assert.IsTrue(processed);
        Assert.AreEqual("test", command);
        Assert.ContainsSingle(parameters);
        Assert.IsNotNull(parameters[0]);
        Assert.AreEqual("2.arg1", parameters[0].RawValue);
        Assert.AreEqual("arg1", parameters[0].Value);
        Assert.AreEqual(2, parameters[0].Count);
        Assert.IsFalse(parameters[0].IsNumber);
        Assert.IsFalse(parameters[0].IsLong);
        Assert.IsFalse(parameters[0].IsAll);
    }

    [TestMethod]
    public void MultipleArgs_Count()
    {
        var Parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var processed = Parser.ExtractCommandAndParameters("test 2'.arg1' arg3", out var command, out var parameters);

        Assert.IsTrue(processed);
        Assert.AreEqual("test", command);
        Assert.HasCount(2, parameters);
        Assert.AreEqual("2.arg1", parameters[0].RawValue);
        Assert.AreEqual("arg1", parameters[0].Value);
        Assert.AreEqual(2, parameters[0].Count);
        Assert.IsFalse(parameters[0].IsNumber);
        Assert.IsFalse(parameters[0].IsLong);
        Assert.IsFalse(parameters[0].IsAll);
        Assert.AreEqual("arg3", parameters[1].RawValue);
        Assert.AreEqual("arg3", parameters[1].Value);
        Assert.AreEqual(1, parameters[1].Count);
        Assert.IsFalse(parameters[1].IsNumber);
        Assert.IsFalse(parameters[1].IsLong);
        Assert.IsFalse(parameters[1].IsAll);
    }

    [TestMethod]
    public void MultipleArgs_Count_2()
    {
        var Parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var processed = Parser.ExtractCommandAndParameters("test 2.'arg1 arg2' 3.arg3 5.arg4", out var command, out var parameters);

        Assert.IsTrue(processed);
        Assert.AreEqual("test", command);
        Assert.HasCount(3, parameters);
        Assert.AreEqual("2.arg1 arg2", parameters[0].RawValue);
        Assert.AreEqual("arg1 arg2", parameters[0].Value);
        Assert.AreEqual(2, parameters[0].Count);
        Assert.IsFalse(parameters[0].IsNumber);
        Assert.IsFalse(parameters[0].IsLong);
        Assert.IsFalse(parameters[0].IsAll);
        Assert.AreEqual("3.arg3", parameters[1].RawValue);
        Assert.AreEqual("arg3", parameters[1].Value);
        Assert.AreEqual(3, parameters[1].Count);
        Assert.IsFalse(parameters[1].IsNumber);
        Assert.IsFalse(parameters[1].IsLong);
        Assert.IsFalse(parameters[1].IsAll);
        Assert.AreEqual("5.arg4", parameters[2].RawValue);
        Assert.AreEqual("arg4", parameters[2].Value);
        Assert.AreEqual(5, parameters[2].Count);
        Assert.IsFalse(parameters[2].IsNumber);
        Assert.IsFalse(parameters[2].IsLong);
        Assert.IsFalse(parameters[2].IsAll);
    }

    [TestMethod]
    public void SingleArg_Count_Invalid()
    {
        var Parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var processed = Parser.ExtractCommandAndParameters("test 2.", out var command, out var parameters);

        Assert.IsFalse(processed);
    }

    [TestMethod]
    public void SingleArg_Count_Invalid_2()
    {
        var Parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var processed = Parser.ExtractCommandAndParameters("test .", out var command, out var parameters);

        Assert.IsFalse(processed);
    }

    [TestMethod]
    public void SingleArg_Count_4()
    {
        var Parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var processed = Parser.ExtractCommandAndParameters("test '2.arg1'", out var command, out var parameters);

        Assert.IsTrue(processed);
        Assert.AreEqual("test", command);
        Assert.ContainsSingle(parameters);
        Assert.IsNotNull(parameters[0]);
        Assert.AreEqual("2.arg1", parameters[0].RawValue);
        Assert.AreEqual("arg1", parameters[0].Value);
        Assert.AreEqual(2, parameters[0].Count);
        Assert.IsFalse(parameters[0].IsNumber);
        Assert.IsFalse(parameters[0].IsLong);
        Assert.IsFalse(parameters[0].IsAll);
    }

    [TestMethod]
    public void NoArg_OutOfGame()
    {
        var Parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var processed = Parser.ExtractCommandAndParameters("/test", out var command, out var parameters);

        Assert.IsTrue(processed);
        Assert.AreEqual("test", command);
        Assert.IsEmpty(parameters);
    }

    [TestMethod]
    public void NoArg_DetectOutOfGame()
    {
        var Parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var processed = Parser.ExtractCommandAndParameters(null, "/test", out var command, out var parameters, out var forceOutOfGame);

        Assert.IsTrue(processed);
        Assert.AreEqual("test", command);
        Assert.IsEmpty(parameters);
        Assert.IsTrue(forceOutOfGame);
    }

    [TestMethod]
    public void NoArg_DetectOutOfGame_2()
    {
        var Parser = new Parser.Parser(new Mock<ILogger<Parser.Parser>>().Object);

        var processed = Parser.ExtractCommandAndParameters(null, "test", out var command, out var parameters, out var forceOutOfGame);

        Assert.IsTrue(processed);
        Assert.AreEqual("test", command);
        Assert.IsEmpty(parameters);
        Assert.IsFalse(forceOutOfGame);
    }
}
