using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.POC.GroupsPetsFollowers;

namespace Mud.POC.Tests.Pets
{
    [TestClass]
    public class RemovePetTests : PetTestBase
    {
        [TestMethod]
        public void RemovePet()
        {
            IPlayableCharacter player1 = new PlayableCharacter("player1", new Mock<IRoom>().Object);
            Mock<INonPlayableCharacter> pet1 = CreatePetMock("pet1");
            player1.AddPet(pet1.Object);

            player1.RemovePet(pet1.Object);

            Assert.AreEqual(0, player1.Pets.Count());
            Assert.IsNull(pet1.Object.Master);
            pet1.Verify(x => x.ChangeMaster(It.IsAny<IPlayableCharacter>()), Times.Exactly(2));
        }

        [TestMethod]
        public void RemovePet_NotAPet()
        {
            IPlayableCharacter player1 = new PlayableCharacter("player1", new Mock<IRoom>().Object);
            Mock<INonPlayableCharacter> pet1 = CreatePetMock("pet1");

            player1.RemovePet(pet1.Object);

            Assert.AreEqual(0, player1.Pets.Count());
            Assert.IsNull(pet1.Object.Master);
            pet1.Verify(x => x.ChangeMaster(It.IsAny<IPlayableCharacter>()), Times.Never);
        }

        [TestMethod]
        public void RemovePet_MultiplePet_RemoveOne()
        {
            IPlayableCharacter player1 = new PlayableCharacter("player1",  new Mock<IRoom>().Object);
            Mock<INonPlayableCharacter> pet1 = CreatePetMock("pet1");
            Mock<INonPlayableCharacter> pet2 = CreatePetMock("pet2");
            Mock<INonPlayableCharacter> pet3 = CreatePetMock("pet3");
            player1.AddPet(pet1.Object);
            player1.AddPet(pet2.Object);
            player1.AddPet(pet3.Object);

            player1.RemovePet(pet2.Object);

            Assert.AreEqual(2, player1.Pets.Count());
            Assert.AreEqual(player1, pet1.Object.Master);
            Assert.IsNull(pet2.Object.Master);
            Assert.AreEqual(player1, pet3.Object.Master);
            pet1.Verify(x => x.ChangeMaster(It.IsAny<IPlayableCharacter>()), Times.Once);
            pet2.Verify(x => x.ChangeMaster(It.IsAny<IPlayableCharacter>()), Times.Exactly(2));
            pet3.Verify(x => x.ChangeMaster(It.IsAny<IPlayableCharacter>()), Times.Once);
        }

        [TestMethod]
        public void RemovePet_RemovePetOfAnotherPlayer()
        {
            IPlayableCharacter player1 = new PlayableCharacter("player1",  new Mock<IRoom>().Object);
            Mock<INonPlayableCharacter> pet1 = CreatePetMock("pet1");
            player1.AddPet(pet1.Object);
            IPlayableCharacter player2 = new PlayableCharacter("player1",  new Mock<IRoom>().Object);
            Mock<INonPlayableCharacter> pet2 = CreatePetMock("pet2");
            player2.AddPet(pet2.Object);

            player1.RemovePet(pet2.Object);

            Assert.AreEqual(1, player1.Pets.Count());
            Assert.AreEqual(1, player2.Pets.Count());
            Assert.AreEqual(player1, pet1.Object.Master);
            Assert.AreEqual(player2, pet2.Object.Master);
            pet1.Verify(x => x.ChangeMaster(It.IsAny<IPlayableCharacter>()), Times.Once);
            pet2.Verify(x => x.ChangeMaster(It.IsAny<IPlayableCharacter>()), Times.Once);
        }
    }
}
