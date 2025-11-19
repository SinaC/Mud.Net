using Microsoft.Extensions.Logging;
using Moq;
using Mud.Common;
using Mud.Server.Ability;
using Mud.Server.Interfaces;
using Mud.Server.Random;
using Mud.Server.Rom24.Spells;

namespace Mud.Server.Rom24.Tests.Abilities;

[TestClass]
public class AbilityManagerTests : AbilityTestBase
{
    protected override void RegisterAdditionalDependencies(Mock<IServiceProvider> serviceProviderMock)
    {
        // SpellBase needs IRandomManager to check if cast loses his/her/its concentration
        Mock<ILogger<AcidBlast>> logger = new Mock<ILogger<AcidBlast>>();
        Mock<IRandomManager> randomManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(x => true);
        randomManagerMock.Setup(x => x.Dice(It.IsAny<int>(), It.IsAny<int>())).Returns<int, int>((count, value) => count * value);
        serviceProviderMock.Setup(x => x.GetService(typeof(AcidBlast)))
            .Returns(new AcidBlast(logger.Object, randomManagerMock.Object));
    }

    [TestMethod]
    public void Ctor_AbilitiesFound()
    {
        Mock<IAssemblyHelper> assemblyHelperMock = new();
        assemblyHelperMock.SetupGet(x => x.AllReferencedAssemblies).Returns(typeof(AcidBlast).Assembly.Yield());
        AbilityManager abilityManager = new (new Mock<ILogger<AbilityManager>>().Object, _serviceProvider, assemblyHelperMock.Object);

        Assert.IsTrue(abilityManager.Abilities.Any());
    }

    [TestMethod]
    public void Indexer_ExistingAbility()
    {
        Mock<IAssemblyHelper> assemblyHelperMock = new Mock<IAssemblyHelper>();
        assemblyHelperMock.SetupGet(x => x.AllReferencedAssemblies).Returns(typeof(AcidBlast).Assembly.Yield());
        AbilityManager abilityManager = new (new Mock<ILogger<AbilityManager>>().Object, _serviceProvider, assemblyHelperMock.Object);

        var abilityInfo = abilityManager["Acid Blast"];

        Assert.IsNotNull(abilityInfo);
    }

    [TestMethod]
    public void Indexer_NonExistingAbility()
    {
        Mock<IAssemblyHelper> assemblyHelperMock = new();
        assemblyHelperMock.SetupGet(x => x.AllReferencedAssemblies).Returns(typeof(AcidBlast).Assembly.Yield());
        AbilityManager abilityManager = new (new Mock<ILogger<AbilityManager>>().Object, _serviceProvider, assemblyHelperMock.Object);

        var abilityInfo = abilityManager["Pouet"];

        Assert.IsNull(abilityInfo);
    }

    [TestMethod]
    public void DependencyContainer_CreateAbilityInstance()
    {
        Mock<IAssemblyHelper> assemblyHelperMock = new();
        assemblyHelperMock.SetupGet(x => x.AllReferencedAssemblies).Returns(typeof(AcidBlast).Assembly.Yield());
        AbilityManager abilityManager = new (new Mock<ILogger<AbilityManager>>().Object, _serviceProvider, assemblyHelperMock.Object);

        var abilityInfo = abilityManager["Acid Blast"];

        var abilityInstance = _serviceProvider.GetService(abilityInfo!.AbilityExecutionType);

        Assert.IsNotNull(abilityInstance);
    }
}
