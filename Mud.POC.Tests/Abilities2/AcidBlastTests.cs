using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Container;
using Mud.POC.Abilities2;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Tests.Abilities2
{
    [TestClass]
    public class AcidBlastTests : TestBase
    {
        [TestMethod]
        public void Guards_NoTarget()
        {
            var acidBlastLearned = new AbilityLearned { Name = "Acid Blast" };
            // register RandomManager and Wiznet
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(x => true);
            randomManagerMock.Setup(x => x.Dice(It.IsAny<int>(), It.IsAny<int>())).Returns<int, int>((count, value) => count * value);
            Mock<IWiznet> wiznetMock = new Mock<IWiznet>();
            DependencyContainer.Current.RegisterInstance(randomManagerMock.Object);
            DependencyContainer.Current.RegisterInstance(wiznetMock.Object);
            // create ability manager
            IAbilityManager abilityManager = new AbilityManager();
            // create room and character mock
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            Mock<ICharacter> victimMock = new Mock<ICharacter>();
            casterMock.SetupGet(x => x.Name).Returns("caster");
            casterMock.SetupGet(x => x.Level).Returns(50);
            casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(x => (100, acidBlastLearned));
            victimMock.SetupGet(x => x.Name).Returns("target");
            victimMock.Setup(x => x.SavesSpell(It.IsAny<int>(), It.IsAny<SchoolTypes>())).Returns<int, SchoolTypes>((level, damageType) => false);
            roomMock.SetupGet(x => x.People).Returns(new ICharacter[] { casterMock.Object, victimMock.Object });
            casterMock.SetupGet(x => x.LearnedAbilities).Returns(acidBlastLearned.Yield());
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            //
            Cast cast = new Cast(abilityManager);
            ActionInput actionInput = new ActionInput(casterMock.Object, "'Acid Blast'");

            string result = cast.Guards(actionInput);

            Assert.AreEqual("Cast the spell on whom?", result);
        }

        [TestMethod]
        public void Guards_NotFoundTarget()
        {
            var acidBlastLearned = new AbilityLearned { Name = "Acid Blast" };
            // register RandomManager and Wiznet
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(x => true);
            randomManagerMock.Setup(x => x.Dice(It.IsAny<int>(), It.IsAny<int>())).Returns<int, int>((count, value) => count * value);
            Mock<IWiznet> wiznetMock = new Mock<IWiznet>();
            DependencyContainer.Current.RegisterInstance(randomManagerMock.Object);
            DependencyContainer.Current.RegisterInstance(wiznetMock.Object);
            // create ability manager
            IAbilityManager abilityManager = new AbilityManager();
            // create room and character mock
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            Mock<ICharacter> victimMock = new Mock<ICharacter>();
            casterMock.SetupGet(x => x.Name).Returns("caster");
            casterMock.SetupGet(x => x.Level).Returns(50);
            casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(x => (100, acidBlastLearned));
            victimMock.SetupGet(x => x.Name).Returns("target");
            victimMock.Setup(x => x.SavesSpell(It.IsAny<int>(), It.IsAny<SchoolTypes>())).Returns<int, SchoolTypes>((level, damageType) => false);
            roomMock.SetupGet(x => x.People).Returns(new ICharacter[] { casterMock.Object, victimMock.Object });
            casterMock.SetupGet(x => x.LearnedAbilities).Returns(acidBlastLearned.Yield());
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            //
            Cast cast = new Cast(abilityManager);
            ActionInput actionInput = new ActionInput(casterMock.Object, "'Acid Blast' pouet");

            string result = cast.Guards(actionInput);

            Assert.AreEqual("They aren't here.", result);
        }

        [TestMethod]
        public void Guards_SafeTarget()
        {
            var acidBlastLearned = new AbilityLearned { Name = "Acid Blast" };
            // register RandomManager and Wiznet
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(x => true);
            randomManagerMock.Setup(x => x.Dice(It.IsAny<int>(), It.IsAny<int>())).Returns<int, int>((count, value) => count * value);
            Mock<IWiznet> wiznetMock = new Mock<IWiznet>();
            DependencyContainer.Current.RegisterInstance(randomManagerMock.Object);
            DependencyContainer.Current.RegisterInstance(wiznetMock.Object);
            // create ability manager
            IAbilityManager abilityManager = new AbilityManager();
            // create room and character mock
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            Mock<ICharacter> victimMock = new Mock<ICharacter>();
            casterMock.SetupGet(x => x.Name).Returns("caster");
            casterMock.SetupGet(x => x.Level).Returns(50);
            casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(x => (100, acidBlastLearned));
            victimMock.SetupGet(x => x.Name).Returns("target");
            victimMock.Setup(x => x.SavesSpell(It.IsAny<int>(), It.IsAny<SchoolTypes>())).Returns<int, SchoolTypes>((level, damageType) => false);
            victimMock.Setup(x => x.IsSafe(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            roomMock.SetupGet(x => x.People).Returns(new ICharacter[] { casterMock.Object, victimMock.Object });
            casterMock.SetupGet(x => x.LearnedAbilities).Returns(acidBlastLearned.Yield());
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            //
            Cast cast = new Cast(abilityManager);
            ActionInput actionInput = new ActionInput(casterMock.Object, "'Acid Blast' target");

            string result = cast.Guards(actionInput);

            Assert.AreEqual("Not on that victim.", result);
        }

        [TestMethod]
        public void Guards_NotOnMaster()
        {
            var acidBlastLearned = new AbilityLearned { Name = "Acid Blast" };
            // register RandomManager and Wiznet
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(x => true);
            randomManagerMock.Setup(x => x.Dice(It.IsAny<int>(), It.IsAny<int>())).Returns<int, int>((count, value) => count * value);
            Mock<IWiznet> wiznetMock = new Mock<IWiznet>();
            DependencyContainer.Current.RegisterInstance(randomManagerMock.Object);
            DependencyContainer.Current.RegisterInstance(wiznetMock.Object);
            // create ability manager
            IAbilityManager abilityManager = new AbilityManager();
            // create room and character mock
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<INonPlayableCharacter> casterMock = new Mock<INonPlayableCharacter>();
            Mock<IPlayableCharacter> victimMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("caster");
            casterMock.SetupGet(x => x.Level).Returns(50);
            casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(x => (100, acidBlastLearned));
            casterMock.SetupGet(x => x.CharacterFlags).Returns(CharacterFlags.Charm);
            casterMock.SetupGet(x => x.Master).Returns(victimMock.Object);
            victimMock.SetupGet(x => x.Name).Returns("target");
            victimMock.Setup(x => x.SavesSpell(It.IsAny<int>(), It.IsAny<SchoolTypes>())).Returns<int, SchoolTypes>((level, damageType) => false);
            victimMock.Setup(x => x.IsSafe(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            roomMock.SetupGet(x => x.People).Returns(new ICharacter[] { casterMock.Object, victimMock.Object });
            casterMock.SetupGet(x => x.LearnedAbilities).Returns(acidBlastLearned.Yield());
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            //
            Cast cast = new Cast(abilityManager);
            ActionInput actionInput = new ActionInput(casterMock.Object, "'Acid Blast' target");

            string result = cast.Guards(actionInput);

            Assert.AreEqual("You can't do that on your own follower.", result);
        }

        [TestMethod]
        public void Execute_Spell()
        {
            var acidBlastLearned = new AbilityLearned { Name = "Acid Blast" };
            // register RandomManager and Wiznet
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(x => true);
            randomManagerMock.Setup(x => x.Dice(It.IsAny<int>(), It.IsAny<int>())).Returns<int, int>((count, value) => count * value);
            Mock<IWiznet> wiznetMock = new Mock<IWiznet>();
            DependencyContainer.Current.RegisterInstance(randomManagerMock.Object);
            DependencyContainer.Current.RegisterInstance(wiznetMock.Object);
            // create ability manager
            IAbilityManager abilityManager = new AbilityManager();
            // create room and character mock
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            Mock<ICharacter> victimMock = new Mock<ICharacter>();
            casterMock.SetupGet(x => x.Name).Returns("caster");
            casterMock.SetupGet(x => x.Level).Returns(50);
            casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(x => (100, acidBlastLearned));
            victimMock.SetupGet(x => x.Name).Returns("target");
            victimMock.Setup(x => x.SavesSpell(It.IsAny<int>(), It.IsAny<SchoolTypes>())).Returns<int, SchoolTypes>((level, damageType) => false);
            roomMock.SetupGet(x => x.People).Returns(new ICharacter[] { casterMock.Object, victimMock.Object });
            casterMock.SetupGet(x => x.LearnedAbilities).Returns(acidBlastLearned.Yield());
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            //
            Cast cast = new Cast(abilityManager);
            ActionInput actionInput = new ActionInput(casterMock.Object, "'Acid Blast' target");
            cast.Guards(actionInput);

            cast.Execute(actionInput);

            victimMock.Verify(x => x.AbilityDamage(casterMock.Object, 50 * 12/*level * 12 = acid blast damage formula*/, It.IsAny<SchoolTypes>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
        }
    }
}
