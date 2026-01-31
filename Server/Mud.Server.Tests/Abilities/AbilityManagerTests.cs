using Microsoft.Extensions.Logging;
using Moq;
using Mud.Common;
using Mud.Server.Ability;
using Mud.Server.Tests.Mocking;

namespace Mud.Server.Tests.Abilities;

[TestClass]
public class AbilityManagerTests : AbilityTestBase
{
    [TestMethod]
    public void Ctor_AbilitiesFound()
    {
        Mock<IAssemblyHelper> assemblyHelperMock = new();
        assemblyHelperMock.SetupGet(x => x.AllReferencedAssemblies).Returns(typeof(Rom24AcidBlast).Assembly.Yield());
        var abilityManager = new AbilityManager(new Mock<ILogger<AbilityManager>>().Object, null!, assemblyHelperMock.Object);

        Assert.IsTrue(abilityManager.Abilities.Any());
    }

    [TestMethod]
    public void Indexer_ExistingAbility()
    {
        Mock<IAssemblyHelper> assemblyHelperMock = new();
        assemblyHelperMock.SetupGet(x => x.AllReferencedAssemblies).Returns(typeof(Rom24AcidBlast).Assembly.Yield());
        var abilityManager = new AbilityManager(new Mock<ILogger<AbilityManager>>().Object, null!, assemblyHelperMock.Object);

        var abilityDefinition = abilityManager["Acid Blast"];

        Assert.IsNotNull(abilityDefinition);
    }

    [TestMethod]
    public void Indexer_NonExistingAbility()
    {
        Mock<IAssemblyHelper> assemblyHelperMock = new();
        assemblyHelperMock.SetupGet(x => x.AllReferencedAssemblies).Returns(typeof(Rom24AcidBlast).Assembly.Yield());
        var abilityManager = new AbilityManager(new Mock<ILogger<AbilityManager>>().Object, null!, assemblyHelperMock.Object);

        var abilityDefinition = abilityManager["Pouet"];

        Assert.IsNull(abilityDefinition);
    }
}
