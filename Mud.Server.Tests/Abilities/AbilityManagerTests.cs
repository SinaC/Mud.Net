using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Container;
using Mud.Server.Ability;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;
using Mud.Server.Rom24.Spells;
using System.Linq;
using System.Reflection;

namespace Mud.Server.Tests.Abilities
{
    [TestClass]
    public class AbilityManagerTests : TestBase
    {
        [TestMethod]
        public void Ctor_AbilitiesFound()
        {
            Mock<IAssemblyHelper> assemblyHelperMock = new Mock<IAssemblyHelper>();
            assemblyHelperMock.SetupGet(x => x.ExecutingAssembly).Returns(typeof(AcidBlast).Assembly);
            AbilityManager abilityManager = new AbilityManager(assemblyHelperMock.Object);

            Assert.IsTrue(abilityManager.Abilities.Count() > 0);
        }

        [TestMethod]
        public void Indexer_ExistingAbility()
        {
            Mock<IAssemblyHelper> assemblyHelperMock = new Mock<IAssemblyHelper>();
            assemblyHelperMock.SetupGet(x => x.ExecutingAssembly).Returns(typeof(AcidBlast).Assembly);
            AbilityManager abilityManager = new AbilityManager(assemblyHelperMock.Object);

            IAbilityInfo abilityInfo = abilityManager["Acid Blast"];

            Assert.IsNotNull(abilityInfo);
        }

        [TestMethod]
        public void Indexer_NonExistingAbility()
        {
            Mock<IAssemblyHelper> assemblyHelperMock = new Mock<IAssemblyHelper>();
            assemblyHelperMock.SetupGet(x => x.ExecutingAssembly).Returns(typeof(AcidBlast).Assembly);
            AbilityManager abilityManager = new AbilityManager(assemblyHelperMock.Object);

            IAbilityInfo abilityInfo = abilityManager["Pouet"];

            Assert.IsNull(abilityInfo);
        }

        [TestMethod]
        public void DependencyContainer_Filled()
        {
            Mock<IAssemblyHelper> assemblyHelperMock = new Mock<IAssemblyHelper>();
            assemblyHelperMock.SetupGet(x => x.ExecutingAssembly).Returns(typeof(AcidBlast).Assembly);
            AbilityManager abilityManager = new AbilityManager(assemblyHelperMock.Object);

            foreach (IAbilityInfo abilityInfo in abilityManager.Abilities)
            {
                var instanceProducer = DependencyContainer.Current.GetRegistration(abilityInfo.AbilityExecutionType, false);
                Assert.IsNotNull(instanceProducer);
            }
        }

        [TestMethod]
        public void DependencyContainer_CreateAbilityInstance()
        {
            Mock<IAssemblyHelper> assemblyHelperMock = new Mock<IAssemblyHelper>();
            assemblyHelperMock.SetupGet(x => x.ExecutingAssembly).Returns(typeof(AcidBlast).Assembly);
            AbilityManager abilityManager = new AbilityManager(assemblyHelperMock.Object);

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
