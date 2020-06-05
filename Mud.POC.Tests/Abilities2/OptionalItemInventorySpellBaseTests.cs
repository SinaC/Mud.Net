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
    public class OptionalItemInventorySpellBaseTests : TestBase
    {
        public const string SpellName = "OptionalItemInventorySpellBaseTests_Spell";

        [TestMethod]
        public void Guards_ItemNotFound()
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
            OptionalItemInventorySpellBaseTestsSpell spell = new OptionalItemInventorySpellBaseTestsSpell(randomManagerMock.Object, wiznetMock.Object);
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, "item", new CommandParameter("item", false));

            string result = spell.Guards(abilityActionInput);

            Assert.AreEqual("You are not carrying that.", result);
        }

        [TestMethod]
        public void Guards_NoItem()
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
            OptionalItemInventorySpellBaseTestsSpell spell = new OptionalItemInventorySpellBaseTestsSpell(randomManagerMock.Object, wiznetMock.Object);
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, "", Enumerable.Empty<CommandParameter>().ToArray());

            string result = spell.Guards(abilityActionInput);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Guards_ItemInEquipment()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IWiznet> wiznetMock = new Mock<IWiznet>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IItem> itemMock = new Mock<IItem>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.SetupGet(x => x.Equipments).Returns(new EquippedItem(EquipmentSlots.Chest) { Item = itemMock.Object }.Yield());
            casterMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityPercentage(It.IsAny<IAbility>())).Returns<IAbility>(x => (100, new AbilityLearned { Name = SpellName }));
            itemMock.SetupGet(x => x.Name).Returns("item");
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            OptionalItemInventorySpellBaseTestsSpell spell = new OptionalItemInventorySpellBaseTestsSpell(randomManagerMock.Object, wiznetMock.Object);
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, "item", new CommandParameter("item", false));

            string result = spell.Guards(abilityActionInput);

            Assert.AreEqual("You are not carrying that.", result);
        }

        [TestMethod]
        public void Guards_ItemInRoom()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IWiznet> wiznetMock = new Mock<IWiznet>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IItem> itemMock = new Mock<IItem>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityPercentage(It.IsAny<IAbility>())).Returns<IAbility>(x => (100, new AbilityLearned { Name = SpellName }));
            itemMock.SetupGet(x => x.Name).Returns("item");
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            roomMock.SetupGet(x => x.Content).Returns(itemMock.Object.Yield());
            OptionalItemInventorySpellBaseTestsSpell spell = new OptionalItemInventorySpellBaseTestsSpell(randomManagerMock.Object, wiznetMock.Object);
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, "item", new CommandParameter("item", false));

            string result = spell.Guards(abilityActionInput);

            Assert.AreEqual("You are not carrying that.", result);
        }

        [TestMethod]
        public void Guards_ItemSpecifiedAndFound()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IWiznet> wiznetMock = new Mock<IWiznet>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IItem> itemMock = new Mock<IItem>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.SetupGet(x => x.Inventory).Returns(itemMock.Object.Yield());
            casterMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityPercentage(It.IsAny<IAbility>())).Returns<IAbility>(x => (100, new AbilityLearned { Name = SpellName }));
            itemMock.SetupGet(x => x.Name).Returns("item");
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            OptionalItemInventorySpellBaseTestsSpell spell = new OptionalItemInventorySpellBaseTestsSpell(randomManagerMock.Object, wiznetMock.Object);
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, "item", new CommandParameter("item", false));

            string result = spell.Guards(abilityActionInput);

            Assert.IsNull(result);
        }

        // Spell without specific guards nor invoke
        [Spell(SpellName, AbilityEffects.None)]
        internal class OptionalItemInventorySpellBaseTestsSpell : OptionalItemInventorySpellBase
        {
            public OptionalItemInventorySpellBaseTestsSpell(IRandomManager randomManager, IWiznet wiznet)
                : base(randomManager, wiznet)
            {
            }

            protected override void Invoke()
            {
            }
        }
    }
}
