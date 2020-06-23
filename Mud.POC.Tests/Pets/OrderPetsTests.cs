using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.POC.GroupsPetsFollowers;
using Mud.Server.Interfaces.GameAction;

namespace Mud.POC.Tests.Pets
{
    [TestClass]
    public class OrderPetsTests : PetTestBase
    {
        [TestMethod]
        public void Order_NoPet()
        {
            IPlayableCharacter player1 = new PlayableCharacter("player1", new Mock<IRoom>().Object);

            var args = BuildParameters("all smile");
            CommandExecutionResults result = player1.DoOrder(args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.NoExecution, result);
        }

        [TestMethod]
        public void Order_NoArgument()
        {
            IPlayableCharacter player1 = new PlayableCharacter("player1", new Mock<IRoom>().Object);
            Mock<INonPlayableCharacter> pet1 = CreatePetMock("pet1");
            player1.AddPet(pet1.Object);

            var args = BuildParameters("");
            CommandExecutionResults result = player1.DoOrder(args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.SyntaxErrorNoDisplay, result);
            pet1.Verify(x => x.Order(It.IsAny<string>(), It.IsAny<ICommandParameter[]>()), Times.Never);
        }

        [TestMethod]
        public void Order_OneArgument()
        {
            IPlayableCharacter player1 = new PlayableCharacter("player1", new Mock<IRoom>().Object);
            Mock<INonPlayableCharacter> pet1 = CreatePetMock("pet1");
            player1.AddPet(pet1.Object);

            var args = BuildParameters("pet1");
            CommandExecutionResults result = player1.DoOrder(args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.SyntaxErrorNoDisplay, result);
            pet1.Verify(x => x.Order(It.IsAny<string>(), It.IsAny<ICommandParameter[]>()), Times.Never);
        }

        [TestMethod]
        public void Order_UnknownPet()
        {
            IPlayableCharacter player1 = new PlayableCharacter("player1", new Mock<IRoom>().Object);
            Mock<INonPlayableCharacter> pet1 = CreatePetMock("pet1");
            player1.AddPet(pet1.Object);

            var args = BuildParameters("pet2 smile");
            CommandExecutionResults result = player1.DoOrder(args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.TargetNotFound, result);
            pet1.Verify(x => x.Order(It.IsAny<string>(), It.IsAny<ICommandParameter[]>()), Times.Never);
        }

        [TestMethod]
        public void Order_NoOurPet()
        {
            IPlayableCharacter player1 = new PlayableCharacter("player1", new Mock<IRoom>().Object);
            Mock<INonPlayableCharacter> pet1 = CreatePetMock("pet1");
            player1.AddPet(pet1.Object);
            IPlayableCharacter player2 = new PlayableCharacter("player2", new Mock<IRoom>().Object);
            Mock<INonPlayableCharacter> pet2 = CreatePetMock("pet2");
            player2.AddPet(pet2.Object);

            var args = BuildParameters("pet2 smile");
            CommandExecutionResults result = player1.DoOrder(args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.TargetNotFound, result);
            pet1.Verify(x => x.Order(It.IsAny<string>(), It.IsAny<ICommandParameter[]>()), Times.Never);
            pet2.Verify(x => x.Order(It.IsAny<string>(), It.IsAny<ICommandParameter[]>()), Times.Never);
        }

        [TestMethod]
        public void Order_OnePet()
        {
            IPlayableCharacter player1 = new PlayableCharacter("player1", new Mock<IRoom>().Object);
            Mock<INonPlayableCharacter> pet1 = CreatePetMock("pet1");
            player1.AddPet(pet1.Object);

            var args = BuildParameters("pet1 smile");
            CommandExecutionResults result = player1.DoOrder(args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
            pet1.Verify(x => x.Order(It.IsAny<string>(), It.IsAny<ICommandParameter[]>()), Times.Once);
        }

        [TestMethod]
        public void Order_All_OnlyOnePet()
        {
            IPlayableCharacter player1 = new PlayableCharacter("player1", new Mock<IRoom>().Object);
            Mock<INonPlayableCharacter> pet1 = CreatePetMock("pet1");
            player1.AddPet(pet1.Object);

            var args = BuildParameters("all smile");
            CommandExecutionResults result = player1.DoOrder(args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
            pet1.Verify(x => x.Order(It.IsAny<string>(), It.IsAny<ICommandParameter[]>()), Times.Once);
        }

        [TestMethod]
        public void Order_All_MultiplePets()
        {
            IPlayableCharacter player1 = new PlayableCharacter("player1", new Mock<IRoom>().Object);
            Mock<INonPlayableCharacter> pet1 = CreatePetMock("pet1");
            player1.AddPet(pet1.Object);
            Mock<INonPlayableCharacter> pet2 = CreatePetMock("pet2");
            player1.AddPet(pet2.Object);
            Mock<INonPlayableCharacter> pet3 = CreatePetMock("pet3");
            player1.AddPet(pet3.Object);

            var args = BuildParameters("all smile");
            CommandExecutionResults result = player1.DoOrder(args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
            pet1.Verify(x => x.Order(It.IsAny<string>(), It.IsAny<ICommandParameter[]>()), Times.Once);
            pet2.Verify(x => x.Order(It.IsAny<string>(), It.IsAny<ICommandParameter[]>()), Times.Once);
            pet3.Verify(x => x.Order(It.IsAny<string>(), It.IsAny<ICommandParameter[]>()), Times.Once);
        }

    }
}
