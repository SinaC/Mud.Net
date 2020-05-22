using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.POC.GroupsPetsFollowers;

namespace Mud.POC.Tests.Pets
{
    [TestClass]
    public class AddPetTests : PetTestBase
    {
        [TestMethod]
        public void AddPet()
        {
            IPlayableCharacter player1 = new PlayableCharacter("player1", new Mock<IRoom>().Object);
            Mock<INonPlayableCharacter> pet1 = CreatePetMock("pet1");

            player1.AddPet(pet1.Object);

            Assert.AreEqual(1, player1.Pets.Count());
            Assert.AreSame(player1, pet1.Object.Master);
            pet1.Verify(x => x.ChangeMaster(It.IsAny<IPlayableCharacter>()), Times.Once);
        }

        [TestMethod]
        public void AddPet_Duplicate()
        {
            IPlayableCharacter player1 = new PlayableCharacter("player1", new Mock<IRoom>().Object);
            Mock<INonPlayableCharacter> pet1 = CreatePetMock("pet1");
            player1.AddPet(pet1.Object);

            player1.AddPet(pet1.Object);

            Assert.AreEqual(1, player1.Pets.Count());
            Assert.AreSame(player1, pet1.Object.Master);
            pet1.Verify(x => x.ChangeMaster(It.IsAny<IPlayableCharacter>()), Times.Once);
        }

        [TestMethod]
        public void AddPet_MultiplePet()
        {
            IPlayableCharacter player1 = new PlayableCharacter("player1", new Mock<IRoom>().Object);
            Mock<INonPlayableCharacter> pet1 = CreatePetMock("pet1");
            player1.AddPet(pet1.Object);
            Mock<INonPlayableCharacter> pet2 = CreatePetMock("pet2");

            player1.AddPet(pet2.Object);

            Assert.AreEqual(2, player1.Pets.Count());
            Assert.AreSame(player1, pet1.Object.Master);
            Assert.AreSame(player1, pet2.Object.Master);
            pet1.Verify(x => x.ChangeMaster(It.IsAny<IPlayableCharacter>()), Times.Once);
            pet2.Verify(x => x.ChangeMaster(It.IsAny<IPlayableCharacter>()), Times.Once);
        }

        [TestMethod]
        public void AddPet_PetOfAnotherPlayer()
        {
            IPlayableCharacter player1 = new PlayableCharacter("player1", new Mock<IRoom>().Object);
            Mock<INonPlayableCharacter> pet1 = CreatePetMock("pet1");
            player1.AddPet(pet1.Object);
            IPlayableCharacter player2 = new PlayableCharacter("player2", new Mock<IRoom>().Object);

            player2.AddPet(pet1.Object);

            Assert.AreEqual(1, player1.Pets.Count());
            Assert.AreEqual(0, player2.Pets.Count());
            Assert.AreSame(player1, pet1.Object.Master);
            pet1.Verify(x => x.ChangeMaster(It.IsAny<IPlayableCharacter>()), Times.Once);
        }
    }
}
