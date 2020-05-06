using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Container;
using Mud.Domain;
using Mud.Server.Abilities;
using Mud.Server.Character.PlayableCharacter;
using Mud.Server.Common;

namespace Mud.Server.Tests.Abilities
{
    // TODO: hard to mock for the moment
    [TestClass]
    public class AbilityCheckImproveTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            var container = new SimpleInjector.Container();
            DependencyContainer.SetManualContainer(container);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            DependencyContainer.SetManualContainer(null);
        }

        //[TestMethod]
        //public void NullAbility_Test()
        //{
        //    var randomManagerMock = new Mock<IRandomManager>();
        //    randomManagerMock
        //        .Setup(x => x.Chance(It.IsAny<int>()))
        //        .Returns(true); // always succeed
        //    DependencyContainer.Current.RegisterInstance<IRandomManager>(randomManagerMock.Object);
        //    IPlayableCharacter character = new PlayableCharacter(Guid.NewGuid(), new CharacterData { }, );

        //    bool improved = character.CheckAbilityImprove((KnownAbility)null, true, 1);

        //    Assert.IsFalse(improved);
        //}

        //[TestMethod]
        //public void NotLearned_Test()
        //{
        //    var randomManagerMock = new Mock<IRandomManager>();
        //    randomManagerMock
        //        .Setup(x => x.Chance(It.IsAny<int>()))
        //        .Returns(true); // always succeed
        //    IPlayableCharacter character = new PlayableCharacter(randomManagerMock.Object);
        //    KnownAbility ability = new KnownAbility
        //    {
        //        Ability = new Ability(AbilityKinds.Passive, 1, "test", AbilityTargets.None, 0, AbilityFlags.None, null, null),
        //        Learned = 0,
        //        Level = 1
        //    };

        //    bool improved = character.CheckAbilityImprove(ability, true, 1);

        //    Assert.IsFalse(improved);
        //}

        //[TestMethod]
        //public void FullyLearned_Test()
        //{
        //    var randomManagerMock = new Mock<IRandomManager>();
        //    randomManagerMock
        //        .Setup(x => x.Chance(It.IsAny<int>()))
        //        .Returns(true); // always succeed
        //    IPlayableCharacter character = new PlayableCharacter(randomManagerMock.Object);
        //    KnownAbility ability = new KnownAbility
        //    {
        //        Ability = new Ability(AbilityKinds.Passive, 1, "test", AbilityTargets.None, 0, AbilityFlags.None, null, null),
        //        Learned = 100,
        //        Level = 1
        //    };

        //    bool improved = character.CheckAbilityImprove(ability, true, 1);

        //    Assert.IsFalse(improved);
        //}

        //[TestMethod]
        //public void FailOnFirstChanceTest_Test()
        //{
        //    var randomManagerMock = new Mock<IRandomManager>();
        //    randomManagerMock
        //        .Setup(x => x.Chance(It.IsAny<int>()))
        //        .Returns(false); // always fails
        //    randomManagerMock
        //        .Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>()))
        //        .Returns<int, int>((x, y) => 10000); // always fails
        //    var tableManagerMock = new Mock<IAttributeTableManager>();
        //    tableManagerMock.Setup(x => x.GetLearnPercentage(It.IsAny<ICharacter>())).Returns<ICharacter>(x => 100);
        //    IPlayableCharacter character = new PlayableCharacter(randomManagerMock.Object, new Mock<IAbilityManager>().Object, tableManagerMock.Object);
        //    KnownAbility ability = new KnownAbility
        //    {
        //        Ability = new Ability(AbilityKinds.Passive, 1, "test", AbilityTargets.None, 0, AbilityFlags.None, null, null),
        //        Learned = 10,
        //        Level = 1,
        //        ImproveDifficulityMultiplier = 1
        //    };

        //    bool improved = character.CheckAbilityImprove(ability, true, 1);

        //    Assert.IsFalse(improved);
        //}

        //[TestMethod]
        //public void FailOnSecondChanceTest_Test()
        //{
        //    var randomManagerMock = new Mock<IRandomManager>();
        //    randomManagerMock
        //        .Setup(x => x.Chance(It.IsAny<int>()))
        //        .Returns(false); // always fails
        //    randomManagerMock
        //        .Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>()))
        //        .Returns<int, int>((x, y) => 0); // always succeed
        //    var tableManagerMock = new Mock<IAttributeTableManager>();
        //    tableManagerMock.Setup(x => x.GetLearnPercentage(It.IsAny<ICharacter>())).Returns<ICharacter>(x => 100);
        //    IPlayableCharacter character = new PlayableCharacter(randomManagerMock.Object, new Mock<IAbilityManager>().Object, tableManagerMock.Object);
        //    KnownAbility ability = new KnownAbility
        //    {
        //        Ability = new Ability(AbilityKinds.Passive, 1, "test", AbilityTargets.None, 0, AbilityFlags.None, null, null),
        //        Learned = 10,
        //        Level = 1,
        //        ImproveDifficulityMultiplier = 1
        //    };

        //    bool improved = character.CheckAbilityImprove(ability, true, 1);

        //    Assert.IsFalse(improved);
        //}

        //[TestMethod]
        //public void Succeed_AbilityNotUsedSuccessfully_Test()
        //{
        //    var randomManagerMock = new Mock<IRandomManager>();
        //    randomManagerMock
        //        .Setup(x => x.Chance(It.IsAny<int>()))
        //        .Returns(true); // always succeed
        //    int learnedInc = 33;
        //    randomManagerMock
        //        .Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>()))
        //        .Returns<int, int>((x, y) => x == 1 && y == 3 ? learnedInc : 0); // always succeed, special case if min=1 and max=15
        //    var tableManagerMock = new Mock<IAttributeTableManager>();
        //    tableManagerMock.Setup(x => x.GetLearnPercentage(It.IsAny<ICharacter>())).Returns<ICharacter>(x => 100);
        //    IPlayableCharacter character = new PlayableCharacter(randomManagerMock.Object, new Mock<IAbilityManager>().Object, tableManagerMock.Object);
        //    long baseExp = character.Experience;
        //    KnownAbility ability = new KnownAbility
        //    {
        //        Ability = new Ability(AbilityKinds.Passive, 1, "test", AbilityTargets.None, 0, AbilityFlags.None, null, null),
        //        Learned = 10,
        //        Level = 1,
        //        ImproveDifficulityMultiplier = 2
        //    };
        //    int baseLearned = ability.Learned;
        //    int baseDifficultyMultipler = ability.ImproveDifficulityMultiplier;

        //    bool improved = character.CheckAbilityImprove(ability, false, 3);

        //    Assert.IsTrue(improved);
        //    Assert.AreEqual(baseLearned + 33, ability.Learned);
        //    Assert.AreEqual(baseExp + 2 * baseDifficultyMultipler, character.Experience);
        //    Assert.AreEqual(1, ability.Level);
        //    Assert.AreEqual(baseDifficultyMultipler, ability.ImproveDifficulityMultiplier);
        //}

        //[TestMethod]
        //public void Succeed_AbilityUsedSuccessfully_Test()
        //{
        //    var randomManagerMock = new Mock<IRandomManager>();
        //    randomManagerMock
        //        .Setup(x => x.Chance(It.IsAny<int>()))
        //        .Returns(true); // always succeed
        //    randomManagerMock
        //        .Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>()))
        //        .Returns<int, int>((x, y) => 0); // always succeed
        //    var tableManagerMock = new Mock<IAttributeTableManager>();
        //    tableManagerMock.Setup(x => x.GetLearnPercentage(It.IsAny<ICharacter>())).Returns<ICharacter>(x => 100);
        //    IPlayableCharacter character = new PlayableCharacter(randomManagerMock.Object, new Mock<IAbilityManager>().Object, tableManagerMock.Object);
        //    long baseExp = character.Experience;
        //    KnownAbility ability = new KnownAbility
        //    {
        //        Ability = new Ability(AbilityKinds.Passive, 1, "test", AbilityTargets.None, 0, AbilityFlags.None, null, null),
        //        Learned = 10,
        //        Level = 1,
        //        ImproveDifficulityMultiplier = 2
        //    };
        //    int baseLearned = ability.Learned;
        //    int baseDifficultyMultipler = ability.ImproveDifficulityMultiplier;

        //    bool improved = character.CheckAbilityImprove(ability, true, 3);

        //    Assert.IsTrue(improved);
        //    Assert.AreEqual(baseLearned + 1, ability.Learned);
        //    Assert.AreEqual(baseExp + 2 * baseDifficultyMultipler, character.Experience);
        //    Assert.AreEqual(1, ability.Level);
        //    Assert.AreEqual(baseDifficultyMultipler, ability.ImproveDifficulityMultiplier);
        //}

        //[TestMethod]
        //public void Succeed_AbilityUsedSuccessfully_NegativeMultipliers_Test()
        //{
        //    var randomManagerMock = new Mock<IRandomManager>();
        //    randomManagerMock
        //        .Setup(x => x.Chance(It.IsAny<int>()))
        //        .Returns(true); // always succeed
        //    randomManagerMock
        //        .Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>()))
        //        .Returns<int, int>((x, y) => 0); // always succeed
        //    var tableManagerMock = new Mock<IAttributeTableManager>();
        //    tableManagerMock.Setup(x => x.GetLearnPercentage(It.IsAny<ICharacter>())).Returns<ICharacter>(x => 100);
        //    IPlayableCharacter character = new PlayableCharacter(randomManagerMock.Object, new Mock<IAbilityManager>().Object, tableManagerMock.Object);
        //    long baseExp = character.Experience;
        //    KnownAbility ability = new KnownAbility
        //    {
        //        Ability = new Ability(AbilityKinds.Passive, 1, "test", AbilityTargets.None, 0, AbilityFlags.None, null, null),
        //        Learned = 10,
        //        Level = 1,
        //        ImproveDifficulityMultiplier = -2
        //    };
        //    int baseLearned = ability.Learned;
        //    int baseDifficultyMultipler = ability.ImproveDifficulityMultiplier;

        //    bool improved = character.CheckAbilityImprove(ability, true, -3);

        //    Assert.IsTrue(improved);
        //    Assert.AreEqual(baseLearned + 1, ability.Learned);
        //    Assert.AreEqual(baseExp + 2 * 1, character.Experience);
        //    Assert.AreEqual(1, ability.Level);
        //    Assert.AreEqual(baseDifficultyMultipler, ability.ImproveDifficulityMultiplier);
        //}

        //[TestMethod]
        //public void Succeed_AbilityNotUsedSuccessfully_Max100_Test()
        //{
        //    var randomManagerMock = new Mock<IRandomManager>();
        //    randomManagerMock
        //        .Setup(x => x.Chance(It.IsAny<int>()))
        //        .Returns(true); // always succeed
        //    int learnedInc = 33;
        //    randomManagerMock
        //        .Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>()))
        //        .Returns<int, int>((x, y) => x == 1 && y == 3 ? learnedInc : 0); // always succeed, special case if min=1 and max=15
        //    var tableManagerMock = new Mock<IAttributeTableManager>();
        //    tableManagerMock.Setup(x => x.GetLearnPercentage(It.IsAny<ICharacter>())).Returns<ICharacter>(x => 100);
        //    IPlayableCharacter character = new PlayableCharacter(randomManagerMock.Object, new Mock<IAbilityManager>().Object, tableManagerMock.Object);
        //    long baseExp = character.Experience;
        //    KnownAbility ability = new KnownAbility
        //    {
        //        Ability = new Ability(AbilityKinds.Passive, 1, "test", AbilityTargets.None, 0, AbilityFlags.None, null, null),
        //        Learned = 99,
        //        Level = 1,
        //        ImproveDifficulityMultiplier = 2
        //    };
        //    int baseLearned = ability.Learned;
        //    int baseDifficultyMultipler = ability.ImproveDifficulityMultiplier;

        //    bool improved = character.CheckAbilityImprove(ability, false, 3);

        //    Assert.IsTrue(improved);
        //    Assert.AreEqual(100, ability.Learned); // max 100
        //    Assert.AreEqual(baseExp + 2 * baseDifficultyMultipler, character.Experience);
        //    Assert.AreEqual(1, ability.Level);
        //    Assert.AreEqual(baseDifficultyMultipler, ability.ImproveDifficulityMultiplier);
        //}
    }
}
