﻿using System.Linq;
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
    public class DefensiveSpellBaseSpells
    {
        public const string SpellName = "DefensiveSpellBaseSpells_Spell";

        [TestMethod]
        public void Guards_NoTarget()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IWiznet> wiznetMock = new Mock<IWiznet>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityPercentage(It.IsAny<IAbility>())).Returns<IAbility>(x => (100, new AbilityLearned { Name = SpellName }));
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            DefensiveSpellBaseSpellsSpell spell = new DefensiveSpellBaseSpellsSpell(randomManagerMock.Object, wiznetMock.Object);
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, "", Enumerable.Empty<CommandParameter>().ToArray());

            string result = spell.Guards(abilityActionInput);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Guards_TargetNotFound()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IWiznet> wiznetMock = new Mock<IWiznet>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityPercentage(It.IsAny<IAbility>())).Returns<IAbility>(x => (100, new AbilityLearned { Name = SpellName }));
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            DefensiveSpellBaseSpellsSpell spell = new DefensiveSpellBaseSpellsSpell(randomManagerMock.Object, wiznetMock.Object);
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, "target", new CommandParameter("target", false));

            string result = spell.Guards(abilityActionInput);

            Assert.AreEqual("They aren't here.", result);
        }

        [TestMethod]
        public void Guards_TargetNotFoundInRoom()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IWiznet> wiznetMock = new Mock<IWiznet>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityPercentage(It.IsAny<IAbility>())).Returns<IAbility>(x => (100, new AbilityLearned { Name = SpellName }));
            Mock<ICharacter> victimMock = new Mock<ICharacter>();
            victimMock.SetupGet(x => x.Name).Returns("target");
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            DefensiveSpellBaseSpellsSpell spell = new DefensiveSpellBaseSpellsSpell(randomManagerMock.Object, wiznetMock.Object);
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, "target", new CommandParameter("target", false));

            string result = spell.Guards(abilityActionInput);

            Assert.AreEqual("They aren't here.", result);
        }

        [TestMethod]
        public void Guards_CharacterSpecifiedAndFound()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IWiznet> wiznetMock = new Mock<IWiznet>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityPercentage(It.IsAny<IAbility>())).Returns<IAbility>(x => (100, new AbilityLearned { Name = SpellName }));
            Mock<ICharacter> victimMock = new Mock<ICharacter>();
            victimMock.SetupGet(x => x.Name).Returns("target");
            roomMock.SetupGet(x => x.People).Returns(new ICharacter[] { casterMock.Object, victimMock.Object});
            DefensiveSpellBaseSpellsSpell spell = new DefensiveSpellBaseSpellsSpell(randomManagerMock.Object, wiznetMock.Object);
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, "target", new CommandParameter("target", false));

            string result = spell.Guards(abilityActionInput);

            Assert.IsNull(result);
        }


        // Spell without specific guards nor invoke
        [Spell(SpellName, AbilityEffects.Buff)]
        internal class DefensiveSpellBaseSpellsSpell : DefensiveSpellBase
        {
            public DefensiveSpellBaseSpellsSpell(IRandomManager randomManager, IWiznet wiznet)
                : base(randomManager, wiznet)
            {
            }

            protected override void Invoke()
            {
            }
        }
    }
}
