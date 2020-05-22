using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.POC.Abilities;
using Mud.Server.Common;
using Mud.Server.Input;
using System.Linq;

namespace Mud.POC.Tests.Abilities
{
    [TestClass]
    public class SkillCommandTest : AbilityTestBase
    {
        [TestMethod]
        public void CommandsFound_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            Skills.RandomManager = randomManagerMock.Object;
            var tableManagerMock = new Mock<IAttributeTableManager>();
            tableManagerMock.Setup(x => x.GetLearnPercentage(It.IsAny<ICharacter>())).Returns<int>(x => 100);
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            IPlayableCharacter source = new PlayableCharacter(randomManagerMock.Object, abilityManager, tableManagerMock.Object, new[] { new KnownAbility { Ability = abilityManager["kick"], Level = 1, Learned = 100, Rating = 1 } }, 1000, 1000, 10, Positions.Standing);

            Assert.IsTrue(source.Commands.Count() > 0);
            Assert.IsTrue(source.Commands.Any(x => x.Key == "kick"));
        }

        [TestMethod]
        public void ExecuteCommand_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            Skills.RandomManager = randomManagerMock.Object;
            var tableManagerMock = new Mock<IAttributeTableManager>();
            tableManagerMock.Setup(x => x.GetLearnPercentage(It.IsAny<ICharacter>())).Returns<int>(x => 100);
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            IPlayableCharacter source = new PlayableCharacter(randomManagerMock.Object, abilityManager, tableManagerMock.Object, new[] { new KnownAbility { Ability = abilityManager["kick"], Level = 1, Learned = 100, Rating = 1 } }, 1000, 1000, 10, Positions.Standing);

            string commandLine = "kick";
            CommandHelpers.ExtractCommandAndParameters(commandLine, out string command, out string rawParameters, out CommandParameter[] parameters);
            bool executed = source.ExecuteCommand(command, rawParameters, parameters);

            Assert.IsTrue(executed);
        }
    }
}
