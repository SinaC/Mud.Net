using Microsoft.Extensions.Logging;
using Moq;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Domain;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Room;
using Mud.Server.Random;

namespace Mud.Server.Tests.Abilities;

[TestClass]
public class SpellBaseTests : AbilityTestBase
{
    public const string SpellName = "SpellBaseTests_Spell";

    [TestMethod]
    public void Setup_NoCaster()
    {
        Mock<IRandomManager> randomManagerMock = new();
        SpellBaseTestsSpell spell = new(new Mock<ILogger<SpellBaseTestsSpell>>().Object, randomManagerMock.Object);
        var parameters = BuildParameters("");
        SpellActionInput abilityActionInput = new(new AbilityDefinition( spell.GetType()), null!, 10, null!, parameters);

        var result = spell.Setup(abilityActionInput);

        Assert.IsNotNull(result);
        Assert.AreEqual("Spell must be cast by a character.", result);
    }

    [TestMethod]
    public void Setup_CasterNoInARoom()
    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<ICharacter> characterMock = new();
        SpellBaseTestsSpell spell = new(new Mock<ILogger<SpellBaseTestsSpell>>().Object, randomManagerMock.Object);
        var parameters = BuildParameters("");
        SpellActionInput abilityActionInput = new(new AbilityDefinition( spell.GetType()), characterMock.Object, 10, null!, parameters);

        var result = spell.Setup(abilityActionInput);

        Assert.IsNotNull(result);
        Assert.AreEqual("You are nowhere...", result);
    }

    [TestMethod]
    public void Setup_InCooldown()
    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IRoom> roomMock = new();
        Mock<ICharacter> casterMock = new();
        casterMock.SetupGet(x => x.Name).Returns("player");
        casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        casterMock.Setup(x => x.CooldownPulseLeft(It.IsAny<string>())).Returns<string>(_ => 10);
        roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
        SpellBaseTestsSpell spell = new(new Mock<ILogger<SpellBaseTestsSpell>>().Object, randomManagerMock.Object);
        var parameters = BuildParameters("player");
        SpellActionInput abilityActionInput = new(new AbilityDefinition( spell.GetType()), casterMock.Object, 10, null!, parameters);

        var result = spell.Setup(abilityActionInput);

        Assert.IsNotNull(result);
        Assert.Contains("is in cooldown", result);
    }

    [TestMethod]
    public void Setup_ResourceKindNotAvailable()
    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IRoom> roomMock = new();
        Mock<ICharacter> casterMock = new();
        casterMock.SetupGet(x => x.Name).Returns("player");
        casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(Enumerable.Empty<ResourceKinds>());
        casterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
        roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
        SpellBaseTestsSpell spell = new(new Mock<ILogger<SpellBaseTestsSpell>>().Object, randomManagerMock.Object);
        var parameters = BuildParameters("player");
        SpellActionInput abilityActionInput = new(new AbilityDefinition( spell.GetType()), casterMock.Object, 10, null!, parameters);

        var result = spell.Setup(abilityActionInput);

        Assert.IsNotNull(result);
        Assert.Contains("as resource for the moment.", result);
    }

    [TestMethod]
    public void Setup_NotEnoughResource()
    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IRoom> roomMock = new();
        Mock<ICharacter> casterMock = new();
        casterMock.SetupGet(x => x.Name).Returns("player");
        casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
        casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(10);
        casterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
        roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
        SpellBaseTestsSpell spell = new(new Mock<ILogger<SpellBaseTestsSpell>>().Object, randomManagerMock.Object);
        var parameters = BuildParameters("player");
        SpellActionInput abilityActionInput = new(new AbilityDefinition( spell.GetType()), casterMock.Object, 10, null!, parameters);

        var result = spell.Setup(abilityActionInput);

        Assert.IsNotNull(result);
        Assert.Contains("You don't have enough", result);
    }

    [TestMethod]
    public void Setup_Ok()
    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IRoom> roomMock = new();
        Mock<ICharacter> casterMock = new();
        casterMock.SetupGet(x => x.Name).Returns("player");
        casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
        casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
        casterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
        roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
        SpellBaseTestsSpell spell = new(new Mock<ILogger<SpellBaseTestsSpell>>().Object, randomManagerMock.Object);
        var parameters = BuildParameters("player");
        SpellActionInput abilityActionInput = new(new AbilityDefinition( spell.GetType()), casterMock.Object, 10, null!, parameters);

        var result = spell.Setup(abilityActionInput);

        Assert.IsNull(result);
    }

    //
    [TestMethod]
    public void Execute_LostConcentration()
    {
        Mock<IRandomManager> randomManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => false);
        Mock<IRoom> roomMock = new();
        Mock<ICharacter> casterMock = new();
        casterMock.SetupGet(x => x.Name).Returns("player");
        casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
        casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
        casterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
        roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
        SpellBaseTestsSpell spell = new(new Mock<ILogger<SpellBaseTestsSpell>>().Object, randomManagerMock.Object);
        var parameters = BuildParameters("player");
        SpellActionInput abilityActionInput = new(new AbilityDefinition( spell.GetType()), casterMock.Object, 10, null!, parameters);
        spell.Setup(abilityActionInput);
        string lastSendReceived = null!;
        casterMock.Setup(x => x.Send(It.IsAny<string>(), It.IsAny<object[]>())).Callback<string, object[]>((msg, args) => lastSendReceived = string.Format(msg, args));

        spell.Execute();

        Assert.AreEqual("You lost your concentration.", lastSendReceived);
    }

    [TestMethod]
    public void Execute_Ok()
    {
        Mock<IRandomManager> randomManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
        Mock<IRoom> roomMock = new();
        Mock<IPlayableCharacter> casterMock = new();
        Mock<IPlayer> playerMock = new();
        casterMock.SetupGet(x => x.Name).Returns("player");
        casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
        casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
        casterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
        casterMock.SetupGet(x => x.ImpersonatedBy).Returns(playerMock.Object);
        roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
        SpellBaseTestsSpell spell = new(new Mock<ILogger<SpellBaseTestsSpell>>().Object, randomManagerMock.Object);
        var parameters = BuildParameters("player");
        SpellActionInput abilityActionInput = new(new AbilityDefinition( spell.GetType()), casterMock.Object, 10, null!, parameters);
        spell.Setup(abilityActionInput);

        spell.Execute();

        casterMock.Verify(x => x.UpdateResource(ResourceKinds.Mana, -50), Times.Once);
        casterMock.Verify(x => x.SetCooldown(SpellName, TimeSpan.FromSeconds(10)), Times.Once);
        casterMock.Verify(x => x.CheckAbilityImprove(It.IsAny<string>(), true, 3), Times.Once);
        casterMock.Verify(x => x.SetGlobalCooldown(20), Times.Once);
    }

    // Spell without specific Setup nor invoke
    [Spell(SpellName, AbilityEffects.None, CooldownInSeconds = 10, LearnDifficultyMultiplier = 3, PulseWaitTime = 20)]
    public class SpellBaseTestsSpell : SpellBase
    {
        public SpellBaseTestsSpell(ILogger<SpellBaseTestsSpell> logger, IRandomManager randomManager)
            : base(logger, randomManager)
        {
        }

        protected override string SetTargets(ISpellActionInput spellActionInput) => null!;

        protected override void Invoke()
        {
        }
    }
}
