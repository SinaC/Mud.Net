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
    public class ItemOrDefensiveSpellBaseTests
    {
        public const string SpellName = "ItemOrDefensiveSpellBaseTests_Spell";

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
            ItemOrDefensiveSpellBaseTestsSpell spell = new ItemOrDefensiveSpellBaseTestsSpell(randomManagerMock.Object);
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, 10, false, "", Enumerable.Empty<CommandParameter>().ToArray());

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
            ItemOrDefensiveSpellBaseTestsSpell spell = new ItemOrDefensiveSpellBaseTestsSpell(randomManagerMock.Object);
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, 10, false, "target", new CommandParameter("target", false));

            string result = spell.Setup(abilityActionInput);

            Assert.AreEqual("You don't see that here.", result);
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
            roomMock.SetupGet(x => x.People).Returns(new [] { casterMock.Object, victimMock.Object });
            casterMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            ItemOrDefensiveSpellBaseTestsSpell spell = new ItemOrDefensiveSpellBaseTestsSpell(randomManagerMock.Object);
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, 10, false, "target", new CommandParameter("target", false));

            string result = spell.Setup(abilityActionInput);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Setup_ItemInEquipment()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IItemWeapon> itemMock = new Mock<IItemWeapon>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.SetupGet(x => x.Equipments).Returns(new EquippedItem(EquipmentSlots.Chest) { Item = itemMock.Object }.Yield());
            casterMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(x => (100, new AbilityLearned { Name = SpellName }));
            itemMock.SetupGet(x => x.Name).Returns("item");
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            ItemOrDefensiveSpellBaseTestsSpell spell = new ItemOrDefensiveSpellBaseTestsSpell(randomManagerMock.Object);
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, 10, false, "item", new CommandParameter("item", false));

            string result = spell.Setup(abilityActionInput);

            Assert.AreEqual("You don't see that here.", result);
        }

        [TestMethod]
        public void Setup_ItemInRoom()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IItemWeapon> itemMock = new Mock<IItemWeapon>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(x => (100, new AbilityLearned { Name = SpellName }));
            itemMock.SetupGet(x => x.Name).Returns("item");
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            roomMock.SetupGet(x => x.Content).Returns(itemMock.Object.Yield());
            ItemOrDefensiveSpellBaseTestsSpell spell = new ItemOrDefensiveSpellBaseTestsSpell(randomManagerMock.Object);
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, 10, false, "item", new CommandParameter("item", false));

            string result = spell.Setup(abilityActionInput);

            Assert.AreEqual("You don't see that here.", result);
        }

        [TestMethod]
        public void Setup_ItemSpecifiedAndFound()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IItemWeapon> itemMock = new Mock<IItemWeapon>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.SetupGet(x => x.Inventory).Returns(itemMock.Object.Yield());
            casterMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(x => (100, new AbilityLearned { Name = SpellName }));
            itemMock.SetupGet(x => x.Name).Returns("item");
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            ItemOrDefensiveSpellBaseTestsSpell spell = new ItemOrDefensiveSpellBaseTestsSpell(randomManagerMock.Object);
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, 10, false, "item", new CommandParameter("item", false));

            string result = spell.Setup(abilityActionInput);

            Assert.IsNull(result);
        }

        // Spell without specific Setup nor invoke
        [Spell(SpellName, AbilityEffects.None)]
        internal class ItemOrDefensiveSpellBaseTestsSpell : ItemOrDefensiveSpellBase
        {
            public ItemOrDefensiveSpellBaseTestsSpell(IRandomManager randomManager)
                : base(randomManager)
            {
            }

            protected override void Invoke(ICharacter victim)
            {
            }

            protected override void Invoke(IItem item)
            {
            }
        }
    }
}
