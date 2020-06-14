using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.POC.GroupsPetsFollowers;

namespace Mud.POC.Tests.Pets
{
    [TestClass]
    public class OnRemovedTests : PetTestBase
    {
        [TestMethod]
        public void RemovePet()
        {
            IPlayableCharacter player1 = new PlayableCharacter("player1", new Mock<IRoom>().Object);
            INonPlayableCharacter pet1 = new NonPlayableCharacter("pet1", new Mock<IRoom>().Object);
            player1.AddPet(pet1);

            pet1.OnRemoved();

            Assert.IsNull(pet1.Master);
            Assert.AreEqual(0, player1.Pets.Count());
        }

        [TestMethod]
        public void RemoveMaster()
        {
            IPlayableCharacter player1 = new PlayableCharacter("player1", new Mock<IRoom>().Object);
            INonPlayableCharacter pet1 = new NonPlayableCharacter("pet1", new Mock<IRoom>().Object);
            player1.AddPet(pet1);

            player1.OnRemoved();

            Assert.IsNull(pet1.Master);
            Assert.AreEqual(0, player1.Pets.Count());
        }
    }
}
