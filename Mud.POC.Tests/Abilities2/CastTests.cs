using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Common;
using Mud.Container;
using Mud.POC.Abilities2;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Rom24Skills;
using Mud.POC.Abilities2.Rom24Spells;
using Mud.Server.Common;
using Mud.Server.Random;

namespace Mud.POC.Tests.Abilities2
{
    [TestClass]
    public class CastTests : TestBase
    {
        [TestMethod]
        public void Guards_NoActor()
        {
            AbilityManager abilityManager = new AbilityManager();
            Cast cast = new Cast(abilityManager);
            ActionInput actionInput = new ActionInput(null, "cast acid pouet");

            string result = cast.Guards(actionInput);

            Assert.IsNotNull(result);
            Assert.AreEqual("Cannot cast a spell without an actor!", result);
        }

        [TestMethod]
        public void Guards_ActorNotACharacter()
        {
            Mock<IActor> actorMock = new Mock<IActor>();
            AbilityManager abilityManager = new AbilityManager();
            Cast cast = new Cast(abilityManager);
            ActionInput actionInput = new ActionInput(actorMock.Object, "cast acid pouet");

            string result = cast.Guards(actionInput);

            Assert.IsNotNull(result);
            Assert.AreEqual("Only character are allowed to cast spells!", result);
        }

        [TestMethod]
        public void Guards_NoSpellSpecified()
        {
            Mock<ICharacter> characterMock = new Mock<ICharacter>();
            AbilityManager abilityManager = new AbilityManager();
            Cast cast = new Cast(abilityManager);
            ActionInput actionInput = new ActionInput(characterMock.Object, "cast");

            string result = cast.Guards(actionInput);

            Assert.IsNotNull(result);
            Assert.AreEqual("Cast what ?", result);
        }

        [TestMethod]
        public void Guards_Inexisting()
        {
            Mock<ICharacter> characterMock = new Mock<ICharacter>();
            characterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(abilityName => (100, new AbilityLearned { Name = abilityName }));
            AbilityManager abilityManager = new AbilityManager();
            Cast cast = new Cast(abilityManager);
            ActionInput actionInput = new ActionInput(characterMock.Object, "cast pouet");

            string result = cast.Guards(actionInput);

            Assert.IsNotNull(result);
            Assert.AreEqual("This spell doesn't exist.", result);
        }

        [TestMethod]
        public void Guards_PartialSpellName()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            DependencyContainer.Current.RegisterInstance(randomManagerMock.Object); // needed to resolve Acid Blast instance
            Mock<ICharacter> characterMock = new Mock<ICharacter>();
            characterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(abilityName => (100, new AbilityLearned { Name = abilityName }));
            AbilityManager abilityManager = new AbilityManager();
            Cast cast = new Cast(abilityManager);
            ActionInput actionInput = new ActionInput(characterMock.Object, "cast Acid");

            string result = cast.Guards(actionInput); // will find Acid Blast

            Assert.AreEqual("You are nowhere...", result); // will go until test on caster.Room in SpellBase.SetupFromCast
        }

        [TestMethod]
        public void Guards_QuotedSpellName()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            DependencyContainer.Current.RegisterInstance(randomManagerMock.Object); // needed to resolve Acid Blast instance
            Mock<ICharacter> characterMock = new Mock<ICharacter>();
            characterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(abilityName => (100, new AbilityLearned { Name = abilityName }));
            AbilityManager abilityManager = new AbilityManager();
            Cast cast = new Cast(abilityManager);
            ActionInput actionInput = new ActionInput(characterMock.Object, "cast 'Acid Blast'");

            string result = cast.Guards(actionInput); // will find Acid Blast

            Assert.AreEqual("You are nowhere...", result); // will go until test on caster.Room in SpellBase.SetupFromCast
        }

        [TestMethod]
        public void Guards_MixedCaseSpellName()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            DependencyContainer.Current.RegisterInstance(randomManagerMock.Object); // needed to resolve Acid Blast instance
            Mock<ICharacter> characterMock = new Mock<ICharacter>();
            characterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(abilityName => (100, new AbilityLearned { Name = abilityName }));
            AbilityManager abilityManager = new AbilityManager();
            Cast cast = new Cast(abilityManager);
            ActionInput actionInput = new ActionInput(characterMock.Object, "cast 'acId bLaSt'");

            string result = cast.Guards(actionInput); // will find Acid Blast

            Assert.AreEqual("You are nowhere...", result); // will go until test on caster.Room in SpellBase.SetupFromCast
        }

        [TestMethod]
        public void Guards_AbilityNotInAbilityManager() // should never happen
        {
            Mock<ICharacter> characterMock = new Mock<ICharacter>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            characterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(abilityName => (100, new AbilityLearned { Name = abilityName }));
            Cast cast = new Cast(abilityManagerMock.Object);
            ActionInput actionInput = new ActionInput(characterMock.Object, "cast Acid");

            string result = cast.Guards(actionInput);

            Assert.AreEqual("This spell doesn't exist.", result);
        }

        [TestMethod]
        public void Guards_AbilityNotInDependencyContainer() // should never happen
        {
            Mock<ICharacter> characterMock = new Mock<ICharacter>();
            characterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(abilityName => (100, new AbilityLearned { Name = abilityName }));
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            abilityManagerMock.Setup(x => x.Search(It.IsAny<string>(), It.IsAny<AbilityTypes>())).Returns<string,AbilityTypes>((_1,_2) => new AbilityInfo(typeof(AcidBlast)));
            Cast cast = new Cast(abilityManagerMock.Object);
            ActionInput actionInput = new ActionInput(characterMock.Object, "cast Acid");

            string result = cast.Guards(actionInput);

            Assert.AreEqual("Spell not found in DependencyContainer!", result);
        }
    }
}
