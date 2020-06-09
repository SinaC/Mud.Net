using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.POC.Abilities2;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System.Linq;
using Mud.POC.Abilities2.Rom24Spells;

namespace Mud.POC.Tests.Abilities2
{
    [TestClass]
    public class CastTests : TestBase
    {
        [TestMethod]
        public void Guards_NoActor_Spell()
        {
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Cast cast = new Cast(abilityManagerMock.Object);
            ActionInput actionInput = new ActionInput(null, string.Empty);

            string result = cast.Guards(actionInput);

            Assert.IsNotNull(result);
            Assert.AreEqual("Cannot cast a spell without an actor.", result);
        }

        [TestMethod]
        public void Guards_ActorNotACharacter_Spell()
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
        public void Guards_NoSpellSpecified_Spell()
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
        public void Guards_Inexisting_Spell()
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
        public void Guards_PartialSpellName()
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
        public void Guards_QuotedSpellName()
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
        public void Guards_MixedCaseSpellName()
        {
            Mock<ICharacter> characterMock = new Mock<ICharacter>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            characterMock.SetupGet(x => x.LearnedAbilities).Returns(new AbilityLearned { Name = "Acid Blast" }.Yield());
            Cast cast = new Cast(abilityManagerMock.Object);
            ActionInput actionInput = new ActionInput(characterMock.Object, "'acId bLaSt'");

            string result = cast.Guards(actionInput); // will find Acid Blast

            Assert.AreNotEqual("You don't know any spells of that name.", result);
        }

        [TestMethod]
        public void Guards_AbilityNotInAbilityManager() // should never happen
        {
            Mock<ICharacter> characterMock = new Mock<ICharacter>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            characterMock.SetupGet(x => x.LearnedAbilities).Returns(new AbilityLearned { Name = "Acid Blast" }.Yield());
            Cast cast = new Cast(abilityManagerMock.Object);
            ActionInput actionInput = new ActionInput(characterMock.Object, "Acid");

            string result = cast.Guards(actionInput);

            Assert.AreEqual("Ability not found in AbilityManager", result);
        }

        [TestMethod]
        public void Guards_AbilityNotInDependencyContainer() // should never happen
        {
            Mock<ICharacter> characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.LearnedAbilities).Returns(new AbilityLearned { Name = "Acid Blast" }.Yield());
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            abilityManagerMock.Setup(x => x[It.IsAny<string>()]).Returns<string>(_ => new AbilityInfo(typeof(AcidBlast)));
            Cast cast = new Cast(abilityManagerMock.Object);
            ActionInput actionInput = new ActionInput(characterMock.Object, "Acid");

            string result = cast.Guards(actionInput);

            Assert.AreEqual("Ability not found in DependencyContainer", result);
        }
    }
}
