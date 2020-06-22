using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.POC.Abilities;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Random;

namespace Mud.POC.Tests.Abilities
{
    [TestClass]
    public class AbilityCheckImproveTests
    {
        [TestMethod]
        public void NullAbility_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            var tableManagerMock = new Mock<IAttributeTableManager>();
            tableManagerMock.Setup(x => x.GetLearnPercentage(It.IsAny<ICharacter>())).Returns<int>(x => 100);
            IPlayableCharacter character = new PlayableCharacter(randomManagerMock.Object, new Mock<IAbilityManager>().Object, tableManagerMock.Object, new Mock<IGameActionManager>().Object);

            bool improved = character.CheckAbilityImprove((KnownAbility)null, true, 1);

            Assert.IsFalse(improved);
        }

        [TestMethod]
        public void NotLearned_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            var tableManagerMock = new Mock<IAttributeTableManager>();
            tableManagerMock.Setup(x => x.GetLearnPercentage(It.IsAny<ICharacter>())).Returns<int>(x => 100);
            IPlayableCharacter character = new PlayableCharacter(randomManagerMock.Object, new Mock<IAbilityManager>().Object, tableManagerMock.Object, new Mock<IGameActionManager>().Object);
            KnownAbility ability = new KnownAbility
            {
                Ability = new Ability(AbilityKinds.Passive, 1, "test", AbilityTargets.None, 0, AbilityFlags.None, null, null),
                Learned = 0,
                Level = 1
            };

            bool improved = character.CheckAbilityImprove(ability, true, 1);

            Assert.IsFalse(improved);
        }

        [TestMethod]
        public void FullyLearned_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            var tableManagerMock = new Mock<IAttributeTableManager>();
            tableManagerMock.Setup(x => x.GetLearnPercentage(It.IsAny<ICharacter>())).Returns<int>(x => 100);
            IPlayableCharacter character = new PlayableCharacter(randomManagerMock.Object, new Mock<IAbilityManager>().Object, tableManagerMock.Object, new Mock<IGameActionManager>().Object);
            KnownAbility ability = new KnownAbility
            {
                Ability = new Ability(AbilityKinds.Passive, 1, "test", AbilityTargets.None, 0, AbilityFlags.None, null, null),
                Learned = 100,
                Level = 1
            };

            bool improved = character.CheckAbilityImprove(ability, true, 1);

            Assert.IsFalse(improved);
        }

        [TestMethod]
        public void FailOnFirstChanceTest_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(false); // always fails
            randomManagerMock
                .Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>()))
                .Returns<int, int>((x, y) => 10000); // always fails
            var tableManagerMock = new Mock<IAttributeTableManager>();
            tableManagerMock.Setup(x => x.GetLearnPercentage(It.IsAny<ICharacter>())).Returns<ICharacter>(x => 100);
            IPlayableCharacter character = new PlayableCharacter(randomManagerMock.Object, new Mock<IAbilityManager>().Object, tableManagerMock.Object, new Mock<IGameActionManager>().Object);
            KnownAbility ability = new KnownAbility
            {
                Ability = new Ability(AbilityKinds.Passive, 1, "test", AbilityTargets.None, 0, AbilityFlags.None, null, null),
                Learned = 10,
                Level = 1,
                Rating = 1
            };

            bool improved = character.CheckAbilityImprove(ability, true, 1);

            Assert.IsFalse(improved);
        }

        [TestMethod]
        public void FailOnSecondChanceTest_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(false); // always fails
            randomManagerMock
                .Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>()))
                .Returns<int, int>((x, y) => 0); // always succeed
            var tableManagerMock = new Mock<IAttributeTableManager>();
            tableManagerMock.Setup(x => x.GetLearnPercentage(It.IsAny<ICharacter>())).Returns<ICharacter>(x => 100);
            IPlayableCharacter character = new PlayableCharacter(randomManagerMock.Object, new Mock<IAbilityManager>().Object, tableManagerMock.Object, new Mock<IGameActionManager>().Object);
            KnownAbility ability = new KnownAbility
            {
                Ability = new Ability(AbilityKinds.Passive, 1, "test", AbilityTargets.None, 0, AbilityFlags.None, null, null),
                Learned = 10,
                Level = 1,
                Rating = 1
            };

            bool improved = character.CheckAbilityImprove(ability, true, 1);

            Assert.IsFalse(improved);
        }

        [TestMethod]
        public void Succeed_AbilityNotUsedSuccessfully_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            int learnedInc = 33;
            randomManagerMock
                .Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>()))
                .Returns<int, int>((x, y) => x == 1 && y == 3 ? learnedInc : 0); // always succeed, special case if min=1 and max=15
            var tableManagerMock = new Mock<IAttributeTableManager>();
            tableManagerMock.Setup(x => x.GetLearnPercentage(It.IsAny<ICharacter>())).Returns<ICharacter>(x => 100);
            IPlayableCharacter character = new PlayableCharacter(randomManagerMock.Object, new Mock<IAbilityManager>().Object, tableManagerMock.Object, new Mock<IGameActionManager>().Object);
            long baseExp = character.Experience;
            KnownAbility ability = new KnownAbility
            {
                Ability = new Ability(AbilityKinds.Passive, 1, "test", AbilityTargets.None, 0, AbilityFlags.None, null, null),
                Learned = 10,
                Level = 1,
                Rating = 2
            };
            int baseLearned = ability.Learned;
            int baseDifficultyMultipler = ability.Rating;

            bool improved = character.CheckAbilityImprove(ability, false, 3);

            Assert.IsTrue(improved);
            Assert.AreEqual(baseLearned + 33, ability.Learned);
            Assert.AreEqual(baseExp + 2 * baseDifficultyMultipler, character.Experience);
            Assert.AreEqual(1, ability.Level);
            Assert.AreEqual(baseDifficultyMultipler, ability.Rating);
        }

        [TestMethod]
        public void Succeed_AbilityUsedSuccessfully_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            randomManagerMock
                .Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>()))
                .Returns<int, int>((x, y) => 0); // always succeed
            var tableManagerMock = new Mock<IAttributeTableManager>();
            tableManagerMock.Setup(x => x.GetLearnPercentage(It.IsAny<ICharacter>())).Returns<ICharacter>(x => 100);
            IPlayableCharacter character = new PlayableCharacter(randomManagerMock.Object, new Mock<IAbilityManager>().Object, tableManagerMock.Object, new Mock<IGameActionManager>().Object);
            long baseExp = character.Experience;
            KnownAbility ability = new KnownAbility
            {
                Ability = new Ability(AbilityKinds.Passive, 1, "test", AbilityTargets.None, 0, AbilityFlags.None, null, null),
                Learned = 10,
                Level = 1,
                Rating = 2
            };
            int baseLearned = ability.Learned;
            int baseDifficultyMultipler = ability.Rating;

            bool improved = character.CheckAbilityImprove(ability, true, 3);

            Assert.IsTrue(improved);
            Assert.AreEqual(baseLearned + 1, ability.Learned);
            Assert.AreEqual(baseExp + 2 * baseDifficultyMultipler, character.Experience);
            Assert.AreEqual(1, ability.Level);
            Assert.AreEqual(baseDifficultyMultipler, ability.Rating);
        }

        [TestMethod]
        public void Succeed_AbilityUsedSuccessfully_NegativeMultipliers_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            randomManagerMock
                .Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>()))
                .Returns<int, int>((x, y) => 0); // always succeed
            var tableManagerMock = new Mock<IAttributeTableManager>();
            tableManagerMock.Setup(x => x.GetLearnPercentage(It.IsAny<ICharacter>())).Returns<ICharacter>(x => 100);
            IPlayableCharacter character = new PlayableCharacter(randomManagerMock.Object, new Mock<IAbilityManager>().Object, tableManagerMock.Object, new Mock<IGameActionManager>().Object);
            long baseExp = character.Experience;
            KnownAbility ability = new KnownAbility
            {
                Ability = new Ability(AbilityKinds.Passive, 1, "test", AbilityTargets.None, 0, AbilityFlags.None, null, null),
                Learned = 10,
                Level = 1,
                Rating = -2
            };
            int baseLearned = ability.Learned;
            int baseDifficultyMultipler = ability.Rating;

            bool improved = character.CheckAbilityImprove(ability, true, -3);

            Assert.IsTrue(improved);
            Assert.AreEqual(baseLearned + 1, ability.Learned);
            Assert.AreEqual(baseExp + 2 * 1, character.Experience);
            Assert.AreEqual(1, ability.Level);
            Assert.AreEqual(baseDifficultyMultipler, ability.Rating);
        }

        [TestMethod]
        public void Succeed_AbilityNotUsedSuccessfully_Max100_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            int learnedInc = 33;
            randomManagerMock
                .Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>()))
                .Returns<int, int>((x, y) => x == 1 && y == 3 ? learnedInc : 0); // always succeed, special case if min=1 and max=15
            var tableManagerMock = new Mock<IAttributeTableManager>();
            tableManagerMock.Setup(x => x.GetLearnPercentage(It.IsAny<ICharacter>())).Returns<ICharacter>(x => 100);
            IPlayableCharacter character = new PlayableCharacter(randomManagerMock.Object, new Mock<IAbilityManager>().Object, tableManagerMock.Object, new Mock<IGameActionManager>().Object);
            long baseExp = character.Experience;
            KnownAbility ability = new KnownAbility
            {
                Ability = new Ability(AbilityKinds.Passive, 1, "test", AbilityTargets.None, 0, AbilityFlags.None, null, null),
                Learned = 99,
                Level = 1,
                Rating = 2
            };
            int baseLearned = ability.Learned;
            int baseDifficultyMultipler = ability.Rating;

            bool improved = character.CheckAbilityImprove(ability, false, 3);

            Assert.IsTrue(improved);
            Assert.AreEqual(100, ability.Learned); // max 100
            Assert.AreEqual(baseExp + 2 * baseDifficultyMultipler, character.Experience);
            Assert.AreEqual(1, ability.Level);
            Assert.AreEqual(baseDifficultyMultipler, ability.Rating);
        }
    }
}
