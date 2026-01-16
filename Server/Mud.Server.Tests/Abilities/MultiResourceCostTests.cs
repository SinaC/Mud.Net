using Microsoft.Extensions.Logging;
using Moq;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Domain;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Room;
using Mud.Random;

namespace Mud.Server.Tests.Abilities;

[TestClass]
public class MultiResourceCostTests : AbilityTestBase
{
    public const string SpellName = "Test_Spell";

    [TestMethod]
    public void RageNotAvailableAsResource()
    {
        Mock<IRandomManager> randomManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
        Mock<IRoom> roomMock = new();
        Mock<IPlayableCharacter> casterMock = new();
        casterMock.SetupGet(x => x.Name).Returns("player");
        casterMock.SetupGet(x => x.Position).Returns(Positions.Standing);
        casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        casterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearnedMultiCost(abilityName)));
        casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
        casterMock.SetupGet(x => x.CurrentResourceKinds).Returns([ResourceKinds.Mana]);
        casterMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns(true);
        Mock<ICharacter> victimMock = new();
        victimMock.SetupGet(x => x.Name).Returns("target");
        victimMock.SetupGet(x => x.Keywords).Returns("target".Yield());
        victimMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        roomMock.SetupGet(x => x.People).Returns([casterMock.Object, victimMock.Object]);
        OffensiveSpellBaseSpellsSpell spell = new(new Mock<ILogger<OffensiveSpellBaseSpellsSpell>>().Object, randomManagerMock.Object);

        var parameters = BuildParameters("target");
        SpellActionInput abilityActionInput = new(new AbilityDefinition(spell.GetType(), []), casterMock.Object, 10, null!, parameters);

        var result = spell.Setup(abilityActionInput);

        Assert.AreEqual("You can't use Rage as resource for the moment.", result);
    }

    [TestMethod]
    public void ManaNotAvailableAsResource()
    {
        Mock<IRandomManager> randomManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
        Mock<IRoom> roomMock = new();
        Mock<IPlayableCharacter> casterMock = new();
        casterMock.SetupGet(x => x.Name).Returns("player");
        casterMock.SetupGet(x => x.Position).Returns(Positions.Standing);
        casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        casterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearnedMultiCost(abilityName)));
        casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
        casterMock.SetupGet(x => x.CurrentResourceKinds).Returns([ResourceKinds.Rage]);
        casterMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns(true);
        Mock<ICharacter> victimMock = new();
        victimMock.SetupGet(x => x.Name).Returns("target");
        victimMock.SetupGet(x => x.Keywords).Returns("target".Yield());
        victimMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        roomMock.SetupGet(x => x.People).Returns([casterMock.Object, victimMock.Object]);
        OffensiveSpellBaseSpellsSpell spell = new(new Mock<ILogger<OffensiveSpellBaseSpellsSpell>>().Object, randomManagerMock.Object);

        var parameters = BuildParameters("target");
        SpellActionInput abilityActionInput = new(new AbilityDefinition(spell.GetType(), []), casterMock.Object, 10, null!, parameters);

        var result = spell.Setup(abilityActionInput);

        Assert.AreEqual("You can't use Mana as resource for the moment.", result);
    }

    [TestMethod]
    public void NotEnoughManaNorRage()
    {
        Mock<IRandomManager> randomManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
        Mock<IRoom> roomMock = new();
        Mock<IPlayableCharacter> casterMock = new();
        casterMock.SetupGet(x => x.Name).Returns("player");
        casterMock.SetupGet(x => x.Position).Returns(Positions.Standing);
        casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        casterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearnedMultiCost(abilityName)));
        casterMock.SetupGet(x => x[It.Is<ResourceKinds>(x => x == ResourceKinds.Rage)]).Returns(10);
        casterMock.SetupGet(x => x[It.Is<ResourceKinds>(x => x == ResourceKinds.Mana)]).Returns(10);
        casterMock.SetupGet(x => x.CurrentResourceKinds).Returns([ResourceKinds.Rage, ResourceKinds.Mana]);
        casterMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns(true);
        Mock<ICharacter> victimMock = new();
        victimMock.SetupGet(x => x.Name).Returns("target");
        victimMock.SetupGet(x => x.Keywords).Returns("target".Yield());
        victimMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        roomMock.SetupGet(x => x.People).Returns([casterMock.Object, victimMock.Object]);
        OffensiveSpellBaseSpellsSpell spell = new(new Mock<ILogger<OffensiveSpellBaseSpellsSpell>>().Object, randomManagerMock.Object);

        var parameters = BuildParameters("target");
        SpellActionInput abilityActionInput = new(new AbilityDefinition(spell.GetType(), []), casterMock.Object, 10, null!, parameters);

        var result = spell.Setup(abilityActionInput);

        Assert.AreEqual("You don't have enough Mana.", result); // mana is evaluated first
    }

    [TestMethod]
    public void NotEnoughMana()
    {
        Mock<IRandomManager> randomManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
        Mock<IRoom> roomMock = new();
        Mock<IPlayableCharacter> casterMock = new();
        casterMock.SetupGet(x => x.Name).Returns("player");
        casterMock.SetupGet(x => x.Position).Returns(Positions.Standing);
        casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        casterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearnedMultiCost(abilityName)));
        casterMock.SetupGet(x => x[It.Is<ResourceKinds>(x => x == ResourceKinds.Rage)]).Returns(100);
        casterMock.SetupGet(x => x[It.Is<ResourceKinds>(x => x == ResourceKinds.Mana)]).Returns(10);
        casterMock.SetupGet(x => x.CurrentResourceKinds).Returns([ResourceKinds.Rage, ResourceKinds.Mana]);
        casterMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns(true);
        Mock<ICharacter> victimMock = new();
        victimMock.SetupGet(x => x.Name).Returns("target");
        victimMock.SetupGet(x => x.Keywords).Returns("target".Yield());
        victimMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        roomMock.SetupGet(x => x.People).Returns([casterMock.Object, victimMock.Object]);
        OffensiveSpellBaseSpellsSpell spell = new(new Mock<ILogger<OffensiveSpellBaseSpellsSpell>>().Object, randomManagerMock.Object);

        var parameters = BuildParameters("target");
        SpellActionInput abilityActionInput = new(new AbilityDefinition(spell.GetType(), []), casterMock.Object, 10, null!, parameters);

        var result = spell.Setup(abilityActionInput);

        Assert.AreEqual("You don't have enough Mana.", result);
    }

    [TestMethod]
    public void NotEnoughRage()
    {
        Mock<IRandomManager> randomManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
        Mock<IRoom> roomMock = new();
        Mock<IPlayableCharacter> casterMock = new();
        casterMock.SetupGet(x => x.Name).Returns("player");
        casterMock.SetupGet(x => x.Position).Returns(Positions.Standing);
        casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        casterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearnedMultiCost(abilityName)));
        casterMock.SetupGet(x => x[It.Is<ResourceKinds>(x => x == ResourceKinds.Rage)]).Returns(10);
        casterMock.SetupGet(x => x[It.Is<ResourceKinds>(x => x == ResourceKinds.Mana)]).Returns(100);
        casterMock.SetupGet(x => x.CurrentResourceKinds).Returns([ResourceKinds.Rage, ResourceKinds.Mana]);
        casterMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns(true);
        Mock<ICharacter> victimMock = new();
        victimMock.SetupGet(x => x.Name).Returns("target");
        victimMock.SetupGet(x => x.Keywords).Returns("target".Yield());
        victimMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        roomMock.SetupGet(x => x.People).Returns([casterMock.Object, victimMock.Object]);
        OffensiveSpellBaseSpellsSpell spell = new(new Mock<ILogger<OffensiveSpellBaseSpellsSpell>>().Object, randomManagerMock.Object);

        var parameters = BuildParameters("target");
        SpellActionInput abilityActionInput = new(new AbilityDefinition(spell.GetType(), []), casterMock.Object, 10, null!, parameters);

        var result = spell.Setup(abilityActionInput);

        Assert.AreEqual("You don't have enough Rage.", result);
    }

    [TestMethod]
    public void ResourceUsed()
    {
        Mock<IRandomManager> randomManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
        Mock<IRoom> roomMock = new();
        Mock<IPlayableCharacter> casterMock = new();
        casterMock.SetupGet(x => x.Name).Returns("player");
        casterMock.SetupGet(x => x.Position).Returns(Positions.Standing);
        casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        casterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearnedMultiCost(abilityName)));
        casterMock.SetupGet(x => x[It.Is<ResourceKinds>(x => x == ResourceKinds.Rage)]).Returns(100);
        casterMock.SetupGet(x => x[It.Is<ResourceKinds>(x => x == ResourceKinds.Mana)]).Returns(100);
        casterMock.SetupGet(x => x.CurrentResourceKinds).Returns([ResourceKinds.Rage, ResourceKinds.Mana]);
        casterMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns(true);
        Mock<ICharacter> victimMock = new();
        victimMock.SetupGet(x => x.Name).Returns("target");
        victimMock.SetupGet(x => x.Keywords).Returns("target".Yield());
        victimMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        roomMock.SetupGet(x => x.People).Returns([casterMock.Object, victimMock.Object]);
        OffensiveSpellBaseSpellsSpell spell = new(new Mock<ILogger<OffensiveSpellBaseSpellsSpell>>().Object, randomManagerMock.Object);

        var parameters = BuildParameters("target");
        SpellActionInput abilityActionInput = new(new AbilityDefinition(spell.GetType(), []), casterMock.Object, 10, null!, parameters);

        var result = spell.Setup(abilityActionInput);
        spell.Execute();

        Assert.IsNull(result);
        casterMock.Verify(x => x.UpdateResource(It.Is<ResourceKinds>(rk => rk == ResourceKinds.Mana), It.Is<decimal>(cost => cost == -50)), Times.Once);
        casterMock.Verify(x => x.UpdateResource(It.Is<ResourceKinds>(rk => rk == ResourceKinds.Rage), It.Is<decimal>(cost => cost == -75)), Times.Once);
    }

    private static IAbilityLearned BuildAbilityLearnedMultiCost(string name)
    {
        var mock = new Mock<IAbilityLearned>();
        mock.SetupGet(x => x.AbilityUsage).Returns(new AbilityUsage(name, 1, [new AbilityResourceCost(ResourceKinds.Mana, 50, CostAmountOperators.Fixed), new AbilityResourceCost(ResourceKinds.Rage, 75, CostAmountOperators.Fixed)], 1, 100, null!));
        mock.SetupGet(x => x.Name).Returns(name);
        mock.Setup(x => x.HasCost).Returns(true);
        return mock.Object;
    }

    // Spell without specific Setup nor invoke
    [Spell(SpellName, AbilityEffects.Damage)]
    public class OffensiveSpellBaseSpellsSpell : OffensiveSpellBase
    {
        public OffensiveSpellBaseSpellsSpell(ILogger<OffensiveSpellBaseSpellsSpell> logger, IRandomManager randomManager)
            : base(logger, randomManager)
        {
        }

        protected override void Invoke()
        {
        }
    }
}
