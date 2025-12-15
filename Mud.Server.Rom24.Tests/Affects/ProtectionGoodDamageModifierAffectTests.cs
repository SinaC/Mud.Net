using Moq;
using Mud.Server.Interfaces.Character;
using Mud.Server.Rom24.Affects;

namespace Mud.Server.Rom24.Tests.Affects
{
    [TestClass]
    public class ProtectionGoodDamageModifierAffectTests
    {
        [TestMethod]
        public void IsNotGood()
        {
            var chMock = new Mock<ICharacter>();
            chMock.SetupGet(x => x.IsGood).Returns(false);

            var affect = new ProtectionGoodDamageModifierAffect();
            var modifiedDamage = affect.ModifyDamage(chMock.Object, null!, Domain.SchoolTypes.Pierce, 100);

            Assert.AreEqual(100, modifiedDamage);
        }

        [TestMethod]
        public void IsGood()
        {
            var chMock = new Mock<ICharacter>();
            chMock.SetupGet(x => x.IsGood).Returns(true);

            var affect = new ProtectionGoodDamageModifierAffect();
            var modifiedDamage = affect.ModifyDamage(chMock.Object, null!, Domain.SchoolTypes.Pierce, 100);

            Assert.AreEqual(75, modifiedDamage);
        }
    }
}
