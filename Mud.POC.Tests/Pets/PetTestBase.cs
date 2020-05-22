using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.POC.GroupsPetsFollowers;
using Mud.Server.Input;

namespace Mud.POC.Tests.Pets
{
    [TestClass]
    public abstract class PetTestBase
    {
        [TestInitialize]
        public void TestInitialize()
        {
            World.Instance.Clear();
        }

        protected Mock<INonPlayableCharacter> CreatePetMock(string name)
        {
            Mock<INonPlayableCharacter> npcMock = new Mock<INonPlayableCharacter>();
            npcMock.Setup(x => x.ChangeMaster(It.IsAny<IPlayableCharacter>())).Callback<IPlayableCharacter>(x => npcMock.SetupGet(y => y.Master).Returns(x));
            npcMock.SetupGet(x => x.Name).Returns(name);
            return npcMock;
        }

        protected (string rawParameters, CommandParameter[] parameters) BuildParameters(string parameters)
        {
            var commandParameters = CommandHelpers.SplitParameters(parameters).Select(CommandHelpers.ParseParameter).ToArray();
            return (parameters, commandParameters);
        }

        protected (string rawParameters, CommandParameter[] parameters) BuildParametersSkipFirst(string parameters)
        {
            return CommandHelpers.SkipParameters(CommandHelpers.SplitParameters(parameters).Select(CommandHelpers.ParseParameter), 1);
        }
    }
}
