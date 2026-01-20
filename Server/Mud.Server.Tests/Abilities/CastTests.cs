using Microsoft.Extensions.Logging;
using Moq;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Commands.Character.Ability;
using Mud.Server.Domain;
using Mud.Flags;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Guards;
using Mud.Random;
using Mud.Server.Tests.Mocking;
using System.Reflection;

namespace Mud.Server.Tests.Abilities;

[TestClass]
public class CastTests : AbilityTestBase
{
    private IServiceProvider _serviceProvider = default!;
    private Mock<IGuardGenerator> _guardGeneratorMock = default!;

    [TestInitialize]
    public void TestInitialize()
    {
        var serviceProviderMock = new Mock<IServiceProvider>();
        _serviceProvider = serviceProviderMock.Object;

        var randomManagerMock = new Mock<IRandomManager>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IRandomManager)))
            .Returns(randomManagerMock.Object);
        serviceProviderMock.Setup(x => x.GetService(typeof(Rom24AcidBlast)))
            .Returns(() => new Rom24AcidBlast(new Mock<ILogger<Rom24AcidBlast>>().Object, randomManagerMock.Object));

        _guardGeneratorMock = new Mock<IGuardGenerator>();
    }

    [TestMethod]
    public void Guards_NoCharacter()
    {
        var abilityManager = new AbilityManager(new Mock<ILogger<AbilityManager>>().Object, _serviceProvider, _guardGeneratorMock.Object, new AssemblyHelper());
        var cast = new Cast(new Mock<ILogger<Cast>>().Object, abilityManager);
        var actionInput = BuildActionInput<Cast>(null!, "cast acid pouet");

        var result = cast.Guards(actionInput);

        Assert.IsNotNull(result);
        Assert.AreEqual("This command must be executed by ICharacter", result);
    }

    [TestMethod]
    public void Guards_ActorNotACharacter()
    {
        Mock<IActor> actorMock = new();
        var abilityManager = new AbilityManager(new Mock<ILogger<AbilityManager>>().Object, _serviceProvider, _guardGeneratorMock.Object, new AssemblyHelper());
        var cast = new Cast(new Mock<ILogger<Cast>>().Object, abilityManager);
        var actionInput = BuildActionInput<Cast>(actorMock.Object, "cast acid pouet");

        var result = cast.Guards(actionInput);

        Assert.IsNotNull(result);
        Assert.AreEqual("This command must be executed by ICharacter", result);
    }

    [TestMethod]
    public void Guards_NoSpellSpecified()
    {
        Mock<ICharacter> characterMock = new();
        characterMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
        characterMock.SetupGet(x => x.Position).Returns(Positions.Standing);
        var abilityManager = new AbilityManager(new Mock<ILogger<AbilityManager>>().Object, _serviceProvider, _guardGeneratorMock.Object, new AssemblyHelper());
        var cast = new Cast(new Mock<ILogger<Cast>>().Object, abilityManager);
        var actionInput = BuildActionInput<Cast>(characterMock.Object, "cast");

        var result = cast.Guards(actionInput);

        Assert.IsNotNull(result);
        Assert.AreEqual("Cast what ?", result);
    }

    [TestMethod]
    public void Guards_Inexisting()
    {
        Mock<ICharacter> characterMock = new();
        characterMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
        characterMock.SetupGet(x => x.Position).Returns(Positions.Standing);
        characterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(name => (100, BuildAbilityLearned(name)));
        var abilityManager = new AbilityManager(new Mock<ILogger<AbilityManager>>().Object, _serviceProvider, _guardGeneratorMock.Object, new AssemblyHelper());
        var cast = new Cast(new Mock<ILogger<Cast>>().Object, abilityManager);
        var actionInput = BuildActionInput<Cast>(characterMock.Object, "cast pouet");

        var result = cast.Guards(actionInput);

        Assert.IsNotNull(result);
        Assert.AreEqual("You don't know any spells of that name.", result);
    }

    [TestMethod]
    public void Guards_PartialSpellName()
    {
        Mock<ICharacter> characterMock = new();
        characterMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
        characterMock.SetupGet(x => x.Position).Returns(Positions.Standing);
        characterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(name => (100, BuildAbilityLearned(name)));
        var abilityManager = new AbilityManager(new Mock<ILogger<AbilityManager>>().Object, _serviceProvider, _guardGeneratorMock.Object, new AssemblyHelper());
        var cast = new Cast(new Mock<ILogger<Cast>>().Object, abilityManager);
        var actionInput = BuildActionInput<Cast>(characterMock.Object, "cast Acid");

        var result = cast.Guards(actionInput); // will find Acid Blast

        Assert.AreEqual("You are nowhere...", result); // character is not in a room
    }

    [TestMethod]
    public void Guards_QuotedSpellName()
    {
        Mock<ICharacter> characterMock = new();
        characterMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
        characterMock.SetupGet(x => x.Position).Returns(Positions.Standing);
        characterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(name => (100, BuildAbilityLearned(name)));
        var abilityManager = new AbilityManager(new Mock<ILogger<AbilityManager>>().Object, _serviceProvider, _guardGeneratorMock.Object, new AssemblyHelper());
        var cast = new Cast(new Mock<ILogger<Cast>>().Object, abilityManager);
        var actionInput = BuildActionInput<Cast>(characterMock.Object, "cast 'Acid Blast'");

        var result = cast.Guards(actionInput); // will find Acid Blast

        Assert.AreEqual("You are nowhere...", result); // character is not in a room
    }

    [TestMethod]
    public void Guards_MixedCaseSpellName()
    {
        Mock<ICharacter> characterMock = new();
        characterMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
        characterMock.SetupGet(x => x.Position).Returns(Positions.Standing);
        characterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(name => (100, BuildAbilityLearned(name)));
        var abilityManager = new AbilityManager(new Mock<ILogger<AbilityManager>>().Object, _serviceProvider, _guardGeneratorMock.Object, new AssemblyHelper());
        var cast = new Cast(new Mock<ILogger<Cast>>().Object, abilityManager);
        var actionInput = BuildActionInput<Cast>(characterMock.Object, "cast 'acId bLaSt'");

        var result = cast.Guards(actionInput); // will find Acid Blast

        Assert.AreEqual("You are nowhere...", result); // character is not in a room
    }

    [TestMethod]
    public void Guards_AbilityNotInAbilityManager() // should never happen
    {
        Mock<ICharacter> characterMock = new();
        characterMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
        characterMock.SetupGet(x => x.Position).Returns(Positions.Standing);
        characterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(name => (100, BuildAbilityLearned(name)));
        Mock<IAbilityManager> abilityManagerMock = new();
        var cast = new Cast(new Mock<ILogger<Cast>>().Object, abilityManagerMock.Object);
        var actionInput = BuildActionInput<Cast>(characterMock.Object, "cast Acid");

        var result = cast.Guards(actionInput);

        Assert.AreEqual("You don't know any spells of that name.", result);
    }

    [TestMethod]
    public void Guards_AbilityNotInDependencyContainer() // should never happen
    {
        Mock<ICharacter> characterMock = new();
        characterMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
        characterMock.SetupGet(x => x.Position).Returns(Positions.Standing);
        characterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(name => (100, BuildAbilityLearned(name)));
        Mock<IAbilityManager> abilityManagerMock = new();
        abilityManagerMock.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<AbilityTypes>())).Returns<string, AbilityTypes>((_1, _2) => new AbilityDefinition(typeof(Rom24AcidBlast), []));
        var cast = new Cast(new Mock<ILogger<Cast>>().Object, abilityManagerMock.Object);
        var actionInput = BuildActionInput<Cast>(characterMock.Object, "cast Acid");

        var result = cast.Guards(actionInput);

        Assert.AreEqual("You don't know any spells of that name.", result);
    }

    protected class AssemblyHelper : IAssemblyHelper
    {
        public IEnumerable<Assembly> AllReferencedAssemblies => new[] { typeof(Server.Server).Assembly, typeof(Rom24AcidBlast).Assembly };
    }
}
