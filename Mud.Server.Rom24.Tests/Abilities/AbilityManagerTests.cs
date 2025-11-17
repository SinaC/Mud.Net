using Moq;
using Mud.Common;
using Mud.Server.Ability;
using Mud.Server.Interfaces;
using Mud.Server.Rom24.Spells;

namespace Mud.Server.Rom24.Tests.Abilities;

[TestClass]
public class AbilityManagerTests : AbilityTestBase
{
    [TestMethod]
    public void Ctor_AbilitiesFound()
    {
        Mock<IAssemblyHelper> assemblyHelperMock = new();
        assemblyHelperMock.SetupGet(x => x.AllReferencedAssemblies).Returns(typeof(AcidBlast).Assembly.Yield());
        AbilityManager abilityManager = new (_serviceProvider, assemblyHelperMock.Object);

        Assert.IsTrue(abilityManager.Abilities.Any());
    }

    [TestMethod]
    public void Indexer_ExistingAbility()
    {
        Mock<IAssemblyHelper> assemblyHelperMock = new Mock<IAssemblyHelper>();
        assemblyHelperMock.SetupGet(x => x.AllReferencedAssemblies).Returns(typeof(AcidBlast).Assembly.Yield());
        AbilityManager abilityManager = new (_serviceProvider, assemblyHelperMock.Object);

        var abilityInfo = abilityManager["Acid Blast"];

        Assert.IsNotNull(abilityInfo);
    }

    [TestMethod]
    public void Indexer_NonExistingAbility()
    {
        Mock<IAssemblyHelper> assemblyHelperMock = new();
        assemblyHelperMock.SetupGet(x => x.AllReferencedAssemblies).Returns(typeof(AcidBlast).Assembly.Yield());
        AbilityManager abilityManager = new (_serviceProvider, assemblyHelperMock.Object);

        var abilityInfo = abilityManager["Pouet"];

        Assert.IsNull(abilityInfo);
    }

    [TestMethod]
    public void DependencyContainer_CreateAbilityInstance()
    {
        Mock<IAssemblyHelper> assemblyHelperMock = new();
        assemblyHelperMock.SetupGet(x => x.AllReferencedAssemblies).Returns(typeof(AcidBlast).Assembly.Yield());
        AbilityManager abilityManager = new (_serviceProvider, assemblyHelperMock.Object);

        var abilityInfo = abilityManager["Acid Blast"];

        var abilityInstance = _serviceProvider.GetService(abilityInfo!.AbilityExecutionType);

        Assert.IsNotNull(abilityInstance);
    }
}
