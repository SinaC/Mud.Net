using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Container;
using Mud.POC.Abilities2;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using Mud.Server.Random;
using System.Linq;

namespace Mud.POC.Tests.Abilities2
{
    [TestClass]
    public class AbilityManagerTests : TestBase
    {
        [TestMethod]
        public void Ctor_AbilitiesFound()
        {
            AbilityManager abilityManager = new AbilityManager();

            Assert.IsTrue(abilityManager.Abilities.Count() > 0);
        }

        [TestMethod]
        public void Indexer_ExistingAbility()
        {
            AbilityManager abilityManager = new AbilityManager();

            IAbilityInfo abilityInfo = abilityManager["Acid Blast"];

            Assert.IsNotNull(abilityInfo);
        }

        [TestMethod]
        public void Indexer_NonExistingAbility()
        {
            AbilityManager abilityManager = new AbilityManager();

            IAbilityInfo abilityInfo = abilityManager["Pouet"];

            Assert.IsNull(abilityInfo);
        }

        [TestMethod]
        public void DependencyContainer_Filled()
        {
            AbilityManager abilityManager = new AbilityManager();

            foreach (IAbilityInfo abilityInfo in abilityManager.Abilities)
            {
                var instanceProducer = DependencyContainer.Current.GetRegistration(abilityInfo.AbilityExecutionType, false);
                Assert.IsNotNull(instanceProducer);
            }
        }

        [TestMethod]
        public void DependencyContainer_CreateAbilityInstance()
        {
            AbilityManager abilityManager = new AbilityManager();

            IAbilityInfo abilityInfo = abilityManager["Acid Blast"];

            // Acid Blast needs IRandomManager and IWiznet
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IWiznet> wiznetMock = new Mock<IWiznet>();
            DependencyContainer.Current.RegisterInstance(randomManagerMock.Object);
            DependencyContainer.Current.RegisterInstance(wiznetMock.Object);
            var abilityInstance = DependencyContainer.Current.GetInstance(abilityInfo.AbilityExecutionType);

            Assert.IsNotNull(abilityInstance);
        }
    }
}
