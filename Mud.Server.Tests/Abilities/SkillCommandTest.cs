using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Server.Common;
using Mud.Server.Input;
using System.Linq;
using Mud.Server.Abilities;
using Mud.Server.Character.PlayableCharacter;

namespace Mud.Server.Tests.Abilities
{
    // TODO: hard to mock for the moment
    //[TestClass]
    //public class SkillCommandTest : AbilityTestBase
    //{
    //    [TestMethod]
    //    public void CommandsFound_Test()
    //    {
    //        var randomManagerMock = new Mock<IRandomManager>();
    //        Skills.RandomManager = randomManagerMock.Object;
    //        var tableManagerMock = new Mock<IAttributeTableManager>();
    //        tableManagerMock.Setup(x => x.GetLearnPercentage(It.IsAny<ICharacter>())).Returns<int>(x => 100);
    //        IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
    //        IPlayableCharacter source = new PlayableCharacter(randomManagerMock.Object, abilityManager, tableManagerMock.Object, new[] { new KnownAbility { Ability = abilityManager["kick"], Level = 1, Learned = 100, ImproveDifficulityMultiplier = 1 } }, 1000, 1000, 10, Domain.Positions.Standing);

    //        Assert.IsTrue(source.Commands.Count() > 0);
    //        Assert.IsTrue(source.Commands.Any(x => x.Key == "kick"));
    //    }

    //    [TestMethod]
    //    public void ExecuteCommand_Test()
    //    {
    //        var randomManagerMock = new Mock<IRandomManager>();
    //        Skills.RandomManager = randomManagerMock.Object;
    //        var tableManagerMock = new Mock<IAttributeTableManager>();
    //        tableManagerMock.Setup(x => x.GetLearnPercentage(It.IsAny<ICharacter>())).Returns<int>(x => 100);
    //        IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
    //        IPlayableCharacter source = new PlayableCharacter(randomManagerMock.Object, abilityManager, tableManagerMock.Object, new[] { new KnownAbility { Ability = abilityManager["kick"], Level = 1, Learned = 100, ImproveDifficulityMultiplier = 1 } }, 1000, 1000, 10, Domain.Positions.Standing);

    //        string commandLine = "kick";
    //        CommandHelpers.ExtractCommandAndParameters(commandLine, out string command, out string rawParameters, out CommandParameter[] parameters, out bool forceOutOfGame);
    //        bool executed = source.ExecuteCommand(command, rawParameters, parameters);

    //        Assert.IsTrue(executed);
    //    }
}
