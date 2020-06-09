using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.POC.Abilities2;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.POC.Tests.Abilities2
{
    [TestClass]
    public class DefensiveSpellBaseSpells : TestBase
    {
        public const string SpellName = "DefensiveSpellBaseSpells_Spell";

        [TestMethod]
        public void Setup_NoTarget()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(x => (100, new AbilityLearned { Name = SpellName }));
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            DefensiveSpellBaseSpellsSpell spell = new DefensiveSpellBaseSpellsSpell(randomManagerMock.Object);
            var parameters = BuildParameters("");
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, parameters.rawParameters, parameters.parameters);

            string result = spell.Setup(abilityActionInput);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Setup_TargetNotFound()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(x => (100, new AbilityLearned { Name = SpellName }));
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            DefensiveSpellBaseSpellsSpell spell = new DefensiveSpellBaseSpellsSpell(randomManagerMock.Object);
            var parameters = BuildParameters("target");
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, parameters.rawParameters, parameters.parameters);

            string result = spell.Setup(abilityActionInput);

            Assert.AreEqual("They aren't here.", result);
        }

        [TestMethod]
        public void Setup_TargetNotFoundInRoom()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(x => (100, new AbilityLearned { Name = SpellName }));
            Mock<ICharacter> victimMock = new Mock<ICharacter>();
            victimMock.SetupGet(x => x.Name).Returns("target");
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            DefensiveSpellBaseSpellsSpell spell = new DefensiveSpellBaseSpellsSpell(randomManagerMock.Object);
            var parameters = BuildParameters("target");
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, parameters.rawParameters, parameters.parameters);

            string result = spell.Setup(abilityActionInput);

            Assert.AreEqual("They aren't here.", result);
        }

        [TestMethod]
        public void Setup_CharacterSpecifiedAndFound()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(x => (100, new AbilityLearned { Name = SpellName }));
            Mock<ICharacter> victimMock = new Mock<ICharacter>();
            victimMock.SetupGet(x => x.Name).Returns("target");
            roomMock.SetupGet(x => x.People).Returns(new [] { casterMock.Object, victimMock.Object});
            DefensiveSpellBaseSpellsSpell spell = new DefensiveSpellBaseSpellsSpell(randomManagerMock.Object);
            var parameters = BuildParameters("target");
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, parameters.rawParameters, parameters.parameters);

            string result = spell.Setup(abilityActionInput);

            Assert.IsNull(result);
        }


        // Spell without specific Setup nor invoke
        [Spell(SpellName, AbilityEffects.Buff)]
        internal class DefensiveSpellBaseSpellsSpell : DefensiveSpellBase
        {
            public DefensiveSpellBaseSpellsSpell(IRandomManager randomManager)
                : base(randomManager)
            {
            }

            protected override void Invoke()
            {
            }
        }
    }
}
