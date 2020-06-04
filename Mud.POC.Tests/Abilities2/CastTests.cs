using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Container;
using Mud.POC.Abilities2;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System.Linq;

namespace Mud.POC.Tests.Abilities2
{
    [TestClass]
    public class CastTests : TestBase
    {
        [TestMethod]
        public void Cast_Guards_NoActor_Spell()
        {
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Cast cast = new Cast(abilityManagerMock.Object);
            ActionInput actionInput = new ActionInput(null, string.Empty);

            string result = cast.Guards(actionInput);

            Assert.IsNotNull(result);
            Assert.AreEqual("Cannot cast a spell without an actor.", result);
        }

        [TestMethod]
        public void Cast_Guards_ActorNotACharacter_Spell()
        {
            Mock<IActor> actorMock = new Mock<IActor>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Cast cast = new Cast(abilityManagerMock.Object);
            ActionInput actionInput = new ActionInput(actorMock.Object, string.Empty);

            string result = cast.Guards(actionInput);

            Assert.IsNotNull(result);
            Assert.AreEqual("Only character are allowed to cast spells.", result);
        }

        [TestMethod]
        public void Cast_Guards_NoSpellSpecified_Spell()
        {
            Mock<ICharacter> characterMock = new Mock<ICharacter>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Cast cast = new Cast(abilityManagerMock.Object);
            ActionInput actionInput = new ActionInput(characterMock.Object, string.Empty);

            string result = cast.Guards(actionInput);

            Assert.IsNotNull(result);
            Assert.AreEqual("Cast what ?", result);
        }

        [TestMethod]
        public void Cast_Guards_Inexisting_Spell()
        {
            Mock<ICharacter> characterMock = new Mock<ICharacter>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            characterMock.SetupGet(x => x.LearnedAbilities).Returns(Enumerable.Empty<AbilityLearned>());
            Cast cast = new Cast(abilityManagerMock.Object);
            ActionInput actionInput = new ActionInput(characterMock.Object, "pouet");

            string result = cast.Guards(actionInput);

            Assert.IsNotNull(result);
            Assert.AreEqual("You don't know any spells of that name.", result);
        }

        [TestMethod]
        public void Cast_Guards_PartialSpellName()
        {
            Mock<ICharacter> characterMock = new Mock<ICharacter>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            characterMock.SetupGet(x => x.LearnedAbilities).Returns(new AbilityLearned { Name = "Acid Blast" }.Yield());
            Cast cast = new Cast(abilityManagerMock.Object);
            ActionInput actionInput = new ActionInput(characterMock.Object, "Acid");

            string result = cast.Guards(actionInput); // will find Acid Blast

            Assert.AreNotEqual("You don't know any spells of that name.", result);
        }

        [TestMethod]
        public void Cast_Guards_QuotedSpellName()
        {
            Mock<ICharacter> characterMock = new Mock<ICharacter>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            characterMock.SetupGet(x => x.LearnedAbilities).Returns(new AbilityLearned { Name = "Acid Blast" }.Yield());
            Cast cast = new Cast(abilityManagerMock.Object);
            ActionInput actionInput = new ActionInput(characterMock.Object, "'Acid Blast'");

            string result = cast.Guards(actionInput); // will find Acid Blast

            Assert.AreNotEqual("You don't know any spells of that name.", result);
        }

        [TestMethod]
        public void Cast_Guards_MixedCaseSpellName()
        {
            Mock<ICharacter> characterMock = new Mock<ICharacter>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            characterMock.SetupGet(x => x.LearnedAbilities).Returns(new AbilityLearned { Name = "Acid Blast" }.Yield());
            Cast cast = new Cast(abilityManagerMock.Object);
            ActionInput actionInput = new ActionInput(characterMock.Object, "'acId bLaSt'");

            string result = cast.Guards(actionInput); // will find Acid Blast

            Assert.AreNotEqual("You don't know any spells of that name.", result);
        }

        // TODO: Test AcidBlast guard

        [TestMethod]
        public void Cast_Execute_Spell()
        {
            var acidBlastLearned = new AbilityLearned { Name = "Acid Blast" };
            // register RandomManager and Wiznet
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(x => true);
            randomManagerMock.Setup(x => x.Dice(It.IsAny<int>(), It.IsAny<int>())).Returns<int,int>((count, value) => count*value);
            Mock<IWiznet> wiznetMock = new Mock<IWiznet>();
            DependencyContainer.Current.RegisterInstance(randomManagerMock.Object);
            DependencyContainer.Current.RegisterInstance(wiznetMock.Object);
            // create ability manager
            IAbilityManager abilityManager = new AbilityManager();
            // create room and character mock
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            Mock<ICharacter> victimMock = new Mock<ICharacter>();
            casterMock.SetupGet(x => x.Name).Returns("Sinac");
            casterMock.SetupGet(x => x.Level).Returns(50);
            casterMock.Setup(x => x.GetAbilityPercentage(It.IsAny<IAbility>())).Returns<IAbility>(x => (100, acidBlastLearned));
            victimMock.SetupGet(x => x.Name).Returns("Toto");
            victimMock.Setup(x => x.SavesSpell(It.IsAny<int>(), It.IsAny<SchoolTypes>())).Returns<int, SchoolTypes>((level, damageType) => false);
            roomMock.SetupGet(x => x.People).Returns(new ICharacter[] { casterMock.Object, victimMock.Object});
            casterMock.SetupGet(x => x.LearnedAbilities).Returns(acidBlastLearned.Yield());
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            //
            Cast cast = new Cast(abilityManager);
            ActionInput actionInput = new ActionInput(casterMock.Object, "'Acid Blast' toto");
            cast.Guards(actionInput);

            cast.Execute(actionInput);

            victimMock.Verify(x => x.AbilityDamage(casterMock.Object, It.IsAny<IAbility>(), 50*12/*level * 12 = acid blast damage formula*/, It.IsAny<SchoolTypes>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
        }
    }
}
