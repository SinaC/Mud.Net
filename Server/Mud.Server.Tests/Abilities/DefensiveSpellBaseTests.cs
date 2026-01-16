using Microsoft.Extensions.Logging;
using Moq;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Domain;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Room;
using Mud.Random;

namespace Mud.Server.Tests.Abilities;

[TestClass]
public class DefensiveSpellBaseTests : AbilityTestBase
{
    public const string SpellName = "DefensiveSpellBaseSpells_Spell";

    [TestMethod]
    public void Setup_NoTarget()
    {
        Mock<IRandomManager> randomManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
        Mock<IRoom> roomMock = new();
        Mock<IPlayableCharacter> casterMock = new();
        casterMock.SetupGet(x => x.Name).Returns("player");
        casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        casterMock.SetupGet(x => x.Position).Returns(Positions.Standing);
        casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
        casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
        casterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
        roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
        DefensiveSpellBaseSpellsSpell spell = new(new Mock<ILogger<DefensiveSpellBaseSpellsSpell>>().Object, randomManagerMock.Object);
        var parameters = BuildParameters("");
        SpellActionInput abilityActionInput = new(new AbilityDefinition(spell.GetType(), []), casterMock.Object, 10, null!, parameters);

        var result = spell.Setup(abilityActionInput);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void Setup_TargetNotFound()
    {
        Mock<IRandomManager> randomManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
        Mock<IRoom> roomMock = new();
        Mock<IPlayableCharacter> casterMock = new();
        casterMock.SetupGet(x => x.Name).Returns("player");
        casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        casterMock.SetupGet(x => x.Position).Returns(Positions.Standing);
        casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
        casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
        casterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
        roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
        DefensiveSpellBaseSpellsSpell spell = new(new Mock<ILogger<DefensiveSpellBaseSpellsSpell>>().Object, randomManagerMock.Object);
        var parameters = BuildParameters("target");
        SpellActionInput abilityActionInput = new(new AbilityDefinition(spell.GetType(), []), casterMock.Object, 10, null!, parameters);

        var result = spell.Setup(abilityActionInput);

        Assert.AreEqual("They aren't here.", result);
    }

    [TestMethod]
    public void Setup_TargetNotFoundInRoom()
    {
        Mock<IRandomManager> randomManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
        Mock<IRoom> roomMock = new();
        Mock<IPlayableCharacter> casterMock = new();
        casterMock.SetupGet(x => x.Name).Returns("player");
        casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        casterMock.SetupGet(x => x.Position).Returns(Positions.Standing);
        casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
        casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
        casterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
        Mock<ICharacter> victimMock = new();
        victimMock.SetupGet(x => x.Name).Returns("target");
        victimMock.SetupGet(x => x.Keywords).Returns("target".Yield());
        roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
        DefensiveSpellBaseSpellsSpell spell = new(new Mock<ILogger<DefensiveSpellBaseSpellsSpell>>().Object, randomManagerMock.Object);
        var parameters = BuildParameters("target");
        SpellActionInput abilityActionInput = new(new AbilityDefinition(spell.GetType(), []), casterMock.Object, 10, null!, parameters);

        var result = spell.Setup(abilityActionInput);

        Assert.AreEqual("They aren't here.", result);
    }

    [TestMethod]
    public void Setup_InvalidPosition_Sleeping()
    {
        Mock<IRandomManager> randomManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
        Mock<IRoom> roomMock = new();
        Mock<IPlayableCharacter> casterMock = new();
        casterMock.SetupGet(x => x.Name).Returns("player");
        casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        casterMock.SetupGet(x => x.Position).Returns(Positions.Sleeping);
        casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
        casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
        casterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
        Mock<ICharacter> victimMock = new();
        victimMock.SetupGet(x => x.Name).Returns("target");
        victimMock.SetupGet(x => x.Keywords).Returns("target".Yield());
        roomMock.SetupGet(x => x.People).Returns([casterMock.Object, victimMock.Object]);
        casterMock.Setup(x => x.CanSee(victimMock.Object)).Returns<ICharacter>(_ => true);
        DefensiveSpellBaseSpellsSpell spell = new(new Mock<ILogger<DefensiveSpellBaseSpellsSpell>>().Object, randomManagerMock.Object);
        var parameters = BuildParameters("target");
        SpellActionInput abilityActionInput = new(new AbilityDefinition(spell.GetType(), [new MinPositionGuard(Positions.Standing)]), casterMock.Object, 10, null!, parameters);

        var result = spell.Setup(abilityActionInput);

        Assert.AreEqual("In your dreams, or what?", result);
    }

    [TestMethod]
    public void Setup_InvalidPosition_Resting()
    {
        Mock<IRandomManager> randomManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
        Mock<IRoom> roomMock = new();
        Mock<IPlayableCharacter> casterMock = new();
        casterMock.SetupGet(x => x.Name).Returns("player");
        casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        casterMock.SetupGet(x => x.Position).Returns(Positions.Resting);
        casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
        casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
        casterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
        Mock<ICharacter> victimMock = new();
        victimMock.SetupGet(x => x.Name).Returns("target");
        victimMock.SetupGet(x => x.Keywords).Returns("target".Yield());
        roomMock.SetupGet(x => x.People).Returns([casterMock.Object, victimMock.Object]);
        casterMock.Setup(x => x.CanSee(victimMock.Object)).Returns<ICharacter>(_ => true);
        DefensiveSpellBaseSpellsSpell spell = new(new Mock<ILogger<DefensiveSpellBaseSpellsSpell>>().Object, randomManagerMock.Object);
        var parameters = BuildParameters("target");
        SpellActionInput abilityActionInput = new(new AbilityDefinition(spell.GetType(), [new MinPositionGuard(Positions.Standing)]), casterMock.Object, 10, null!, parameters);

        var result = spell.Setup(abilityActionInput);

        Assert.AreEqual("Nah... You feel too relaxed...", result);
    }

    [TestMethod]
    public void Setup_InvalidPosition_Sitting()
    {
        Mock<IRandomManager> randomManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
        Mock<IRoom> roomMock = new();
        Mock<IPlayableCharacter> casterMock = new();
        casterMock.SetupGet(x => x.Name).Returns("player");
        casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        casterMock.SetupGet(x => x.Position).Returns(Positions.Sitting);
        casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
        casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
        casterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
        Mock<ICharacter> victimMock = new();
        victimMock.SetupGet(x => x.Name).Returns("target");
        victimMock.SetupGet(x => x.Keywords).Returns("target".Yield());
        roomMock.SetupGet(x => x.People).Returns([casterMock.Object, victimMock.Object]);
        casterMock.Setup(x => x.CanSee(victimMock.Object)).Returns<ICharacter>(_ => true);
        DefensiveSpellBaseSpellsSpell spell = new(new Mock<ILogger<DefensiveSpellBaseSpellsSpell>>().Object, randomManagerMock.Object);
        var parameters = BuildParameters("target");
        SpellActionInput abilityActionInput = new(new AbilityDefinition(spell.GetType(), [new MinPositionGuard(Positions.Standing)]), casterMock.Object, 10, null!, parameters);

        var result = spell.Setup(abilityActionInput);

        Assert.AreEqual("Better stand up first.", result);
    }

    [TestMethod]
    public void Setup_CharacterSpecifiedAndFound()
    {
        Mock<IRandomManager> randomManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
        Mock<IRoom> roomMock = new();
        Mock<IPlayableCharacter> casterMock = new();
        casterMock.SetupGet(x => x.Name).Returns("player");
        casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        casterMock.SetupGet(x => x.Position).Returns(Positions.Standing);
        casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
        casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
        casterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
        Mock<ICharacter> victimMock = new();
        victimMock.SetupGet(x => x.Name).Returns("target");
        victimMock.SetupGet(x => x.Keywords).Returns("target".Yield());
        roomMock.SetupGet(x => x.People).Returns([casterMock.Object, victimMock.Object]);
        casterMock.Setup(x => x.CanSee(victimMock.Object)).Returns<ICharacter>(_ => true);
        DefensiveSpellBaseSpellsSpell spell = new(new Mock<ILogger<DefensiveSpellBaseSpellsSpell>>().Object, randomManagerMock.Object);
        var parameters = BuildParameters("target");
        SpellActionInput abilityActionInput = new(new AbilityDefinition(spell.GetType(), []), casterMock.Object, 10, null!, parameters);

        var result = spell.Setup(abilityActionInput);

        Assert.IsNull(result);
    }

    // Spell without specific Setup nor invoke
    [Spell(SpellName, AbilityEffects.Buff)]
    public class DefensiveSpellBaseSpellsSpell : DefensiveSpellBase
    {
        public DefensiveSpellBaseSpellsSpell(ILogger<DefensiveSpellBaseSpellsSpell> logger, IRandomManager randomManager)
            : base(logger, randomManager)
        {
        }

        protected override void Invoke()
        {
        }
    }
}
