using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.POC.Abilities2;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.POC.Tests.Abilities2
{
    [TestClass]
    public class SpellBaseTests : TestBase
    {
        public const string SpellName = "SpellBaseTests_Spell";

        [TestMethod]
        public void Setup_NoAbilityInfo()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<ICharacter> characterMock = new Mock<ICharacter>();
            SpellBaseTestsSpell spell = new SpellBaseTestsSpell(randomManagerMock.Object);
            SpellActionInput abilityActionInput = new SpellActionInput(null, characterMock.Object, 10, null, string.Empty, Enumerable.Empty<CommandParameter>().ToArray());

            string result = spell.Setup(abilityActionInput);

            Assert.AreEqual("Internal error: AbilityInfo is null.", result);
        }

        [TestMethod]
        public void Setup_NoCaster()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            SpellBaseTestsSpell spell = new SpellBaseTestsSpell(randomManagerMock.Object);
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(spell.GetType()), null, 10, null, string.Empty, Enumerable.Empty<CommandParameter>().ToArray());

            string result = spell.Setup(abilityActionInput);

            Assert.AreEqual("Spell must be cast by a character.", result);
        }

        [TestMethod]
        public void Setup_CasterNoInARoom()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<ICharacter> characterMock = new Mock<ICharacter>();
            SpellBaseTestsSpell spell = new SpellBaseTestsSpell(randomManagerMock.Object);
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(spell.GetType()), characterMock.Object, 10, null, string.Empty, Enumerable.Empty<CommandParameter>().ToArray());

            string result = spell.Setup(abilityActionInput);

            Assert.AreEqual("You are nowhere...", result);
        }

        [TestMethod]
        public void Setup_InCooldown()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.CooldownPulseLeft(It.IsAny<string>())).Returns<string>(_ => 10);
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            SpellBaseTestsSpell spell = new SpellBaseTestsSpell(randomManagerMock.Object);
            var parameters = BuildParameters("player");
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, 10, null, parameters.rawParameters, parameters.parameters);

            string result = spell.Setup(abilityActionInput);

            Assert.That.Contains("is in cooldown", result);
        }

        [TestMethod]
        public void Setup_ResourceKindNotAvailable()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(Enumerable.Empty<ResourceKinds>());
            casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(x => (100, new AbilityLearned { Name = SpellName, ResourceKind = ResourceKinds.Mana, CostAmount = 50, CostAmountOperator = CostAmountOperators.Fixed}));
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            SpellBaseTestsSpell spell = new SpellBaseTestsSpell(randomManagerMock.Object);
            var parameters = BuildParameters("player");
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, 10, null, parameters.rawParameters, parameters.parameters);

            string result = spell.Setup(abilityActionInput);

            Assert.That.Contains("as resource for the moment.", result);
        }

        [TestMethod]
        public void Setup_NotEnoughResource()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(10);
            casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(x => (100, new AbilityLearned { Name = SpellName, ResourceKind = ResourceKinds.Mana, CostAmount = 50, CostAmountOperator = CostAmountOperators.Fixed }));
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            SpellBaseTestsSpell spell = new SpellBaseTestsSpell(randomManagerMock.Object);
            var parameters = BuildParameters("player");
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, 10, null, parameters.rawParameters, parameters.parameters);

            string result = spell.Setup(abilityActionInput);

            Assert.That.Contains("You don't have enough", result);
        }

        [TestMethod]
        public void Setup_Ok()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(x => (100, new AbilityLearned { Name = SpellName, ResourceKind = ResourceKinds.Mana, CostAmount = 50, CostAmountOperator = CostAmountOperators.Fixed }));
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            SpellBaseTestsSpell spell = new SpellBaseTestsSpell(randomManagerMock.Object);
            var parameters = BuildParameters("player");
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, 10, null, parameters.rawParameters, parameters.parameters);

            string result = spell.Setup(abilityActionInput);

            Assert.IsNull(result);
        }

        //
        [TestMethod]
        public void Execute_LostConcentration()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => false);
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(x => (100, new AbilityLearned { Name = SpellName, ResourceKind = ResourceKinds.Mana, CostAmount = 50, CostAmountOperator = CostAmountOperators.Fixed }));
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            SpellBaseTestsSpell spell = new SpellBaseTestsSpell(randomManagerMock.Object);
            var parameters = BuildParameters("player");
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, 10, null, parameters.rawParameters, parameters.parameters);
            spell.Setup(abilityActionInput);
            string lastSendReceived = null;
            casterMock.Setup(x => x.Send(It.IsAny<string>(), It.IsAny<object[]>())).Callback<string, object[]>((msg, args) => lastSendReceived = string.Format(msg, args));

            spell.Execute();

            Assert.AreEqual("You lost your concentration.", lastSendReceived);
        }

        [TestMethod]
        public void Execute_Ok()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            Mock<IPlayer> playerMock = new Mock<IPlayer>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(x => (100, new AbilityLearned { Name = SpellName, ResourceKind = ResourceKinds.Mana, CostAmount = 50, CostAmountOperator = CostAmountOperators.Fixed }));
            casterMock.SetupGet(x => x.ImpersonatedBy).Returns(playerMock.Object);
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            SpellBaseTestsSpell spell = new SpellBaseTestsSpell(randomManagerMock.Object);
            var parameters = BuildParameters("player");
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, 10, null, parameters.rawParameters, parameters.parameters);
            spell.Setup(abilityActionInput);

            spell.Execute();

            casterMock.Verify(x => x.UpdateResource(ResourceKinds.Mana, -50), Times.Once);
            casterMock.Verify(x => x.SetCooldown(SpellName, 10), Times.Once);
            casterMock.Verify(x => x.CheckAbilityImprove(It.IsAny<AbilityLearned>(), true, 3), Times.Once);
            playerMock.Verify(x => x.SetGlobalCooldown(20), Times.Once);
        }

        // Spell without specific Setup nor invoke
        [Spell(SpellName, AbilityEffects.None, Cooldown = 10, LearnDifficultyMultiplier = 3, PulseWaitTime = 20)]
        internal class SpellBaseTestsSpell : SpellBase
        {
            public SpellBaseTestsSpell(IRandomManager randomManager)
                : base(randomManager)
            {
            }

            protected override string SetTargets(ISpellActionInput spellActionInput) => null;

            protected override void Invoke()
            {
            }
        }
    }
}
