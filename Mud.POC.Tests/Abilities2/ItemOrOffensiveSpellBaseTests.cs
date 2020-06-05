using System;
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
    public class ItemOrOffensiveSpellBaseTests
    {
        public const string SpellName = "ItemOrOffensiveSpellBaseTests_Spell";

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
            ItemOrOffensiveSpellBaseTestsSpell spell = new ItemOrOffensiveSpellBaseTestsSpell(randomManagerMock.Object, wiznetMock.Object);
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, "", Enumerable.Empty<CommandParameter>().ToArray());

            string result = spell.Guards(abilityActionInput);

            Assert.AreEqual("Cast the spell on whom or what?", result);
        }

        [TestMethod]
        public void Guards_NoTarget_Figthing()
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
            victimMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            roomMock.SetupGet(x => x.People).Returns(new[] { casterMock.Object, victimMock.Object });
            casterMock.SetupGet(x => x.Fighting).Returns(victimMock.Object);
            ItemOrOffensiveSpellBaseTestsSpell spell = new ItemOrOffensiveSpellBaseTestsSpell(randomManagerMock.Object, wiznetMock.Object);
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
            ItemOrOffensiveSpellBaseTestsSpell spell = new ItemOrOffensiveSpellBaseTestsSpell(randomManagerMock.Object, wiznetMock.Object);
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, "target", new CommandParameter("target", false));

            string result = spell.Guards(abilityActionInput);

            Assert.AreEqual("You don't see that here.", result);
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
            ItemOrOffensiveSpellBaseTestsSpell spell = new ItemOrOffensiveSpellBaseTestsSpell(randomManagerMock.Object, wiznetMock.Object);
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, "target", new CommandParameter("target", false));

            string result = spell.Guards(abilityActionInput);

            Assert.AreEqual("You don't see that here.", result);
        }

        [TestMethod]
        public void Guards_SafeTarget()
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
            victimMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            victimMock.Setup(x => x.IsSafe(casterMock.Object)).Returns<ICharacter>(_ => true);
            roomMock.SetupGet(x => x.People).Returns(new[] { casterMock.Object, victimMock.Object });
            casterMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            ItemOrOffensiveSpellBaseTestsSpell spell = new ItemOrOffensiveSpellBaseTestsSpell(randomManagerMock.Object, wiznetMock.Object);
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, "target", new CommandParameter("target", false));

            string result = spell.Guards(abilityActionInput);

            Assert.AreEqual("Not on that victim.", result);
        }

        [TestMethod]
        public void Guards_NotOnMaster()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IWiznet> wiznetMock = new Mock<IWiznet>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IPlayableCharacter> victimMock = new Mock<IPlayableCharacter>();
            victimMock.SetupGet(x => x.Name).Returns("target");
            victimMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            Mock<INonPlayableCharacter> casterMock = new Mock<INonPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityPercentage(It.IsAny<IAbility>())).Returns<IAbility>(x => (100, new AbilityLearned { Name = SpellName }));
            casterMock.SetupGet(x => x.CharacterFlags).Returns(CharacterFlags.Charm);
            casterMock.SetupGet(x => x.Master).Returns(victimMock.Object);
            casterMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            roomMock.SetupGet(x => x.People).Returns(new ICharacter[] { casterMock.Object, victimMock.Object });
            ItemOrOffensiveSpellBaseTestsSpell spell = new ItemOrOffensiveSpellBaseTestsSpell(randomManagerMock.Object, wiznetMock.Object);
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, "target", new CommandParameter("target", false));

            string result = spell.Guards(abilityActionInput);

            Assert.AreEqual("You can't do that on your own follower.", result);
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
            victimMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            roomMock.SetupGet(x => x.People).Returns(new[] { casterMock.Object, victimMock.Object });
            casterMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            ItemOrOffensiveSpellBaseTestsSpell spell = new ItemOrOffensiveSpellBaseTestsSpell(randomManagerMock.Object, wiznetMock.Object);
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, "target", new CommandParameter("target", false));

            string result = spell.Guards(abilityActionInput);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Guards_ItemSpecifiedAndFoundInInventory()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IWiznet> wiznetMock = new Mock<IWiznet>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IItemWeapon> itemMock = new Mock<IItemWeapon>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.SetupGet(x => x.Inventory).Returns(itemMock.Object.Yield());
            casterMock.SetupGet(x => x.Equipments).Returns(new EquippedItem(EquipmentSlots.Chest) { Item = itemMock.Object }.Yield());
            casterMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityPercentage(It.IsAny<IAbility>())).Returns<IAbility>(x => (100, new AbilityLearned { Name = SpellName }));
            itemMock.SetupGet(x => x.Name).Returns("item");
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            roomMock.SetupGet(x => x.Content).Returns(Enumerable.Empty<IItem>());
            ItemOrOffensiveSpellBaseTestsSpell spell = new ItemOrOffensiveSpellBaseTestsSpell(randomManagerMock.Object, wiznetMock.Object);
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, "item", new CommandParameter("item", false));

            string result = spell.Guards(abilityActionInput);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Guards_ItemSpecifiedAndFoundInEquipment()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IWiznet> wiznetMock = new Mock<IWiznet>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IItemWeapon> itemMock = new Mock<IItemWeapon>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.SetupGet(x => x.Equipments).Returns(new EquippedItem(EquipmentSlots.Chest) { Item = itemMock.Object }.Yield());
            casterMock.SetupGet(x => x.Inventory).Returns(Enumerable.Empty<IItem>());
            casterMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityPercentage(It.IsAny<IAbility>())).Returns<IAbility>(x => (100, new AbilityLearned { Name = SpellName }));
            itemMock.SetupGet(x => x.Name).Returns("item");
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            roomMock.SetupGet(x => x.Content).Returns(Enumerable.Empty<IItem>());
            ItemOrOffensiveSpellBaseTestsSpell spell = new ItemOrOffensiveSpellBaseTestsSpell(randomManagerMock.Object, wiznetMock.Object);
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, "item", new CommandParameter("item", false));

            string result = spell.Guards(abilityActionInput);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Guards_ItemSpecifiedAndFoundInRoom()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IWiznet> wiznetMock = new Mock<IWiznet>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IItemWeapon> itemMock = new Mock<IItemWeapon>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.SetupGet(x => x.Equipments).Returns(new EquippedItem(EquipmentSlots.Chest) { Item = itemMock.Object }.Yield());
            casterMock.SetupGet(x => x.Inventory).Returns(Enumerable.Empty<IItem>());
            casterMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityPercentage(It.IsAny<IAbility>())).Returns<IAbility>(x => (100, new AbilityLearned { Name = SpellName }));
            itemMock.SetupGet(x => x.Name).Returns("item");
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            roomMock.SetupGet(x => x.Content).Returns(itemMock.Object.Yield());
            ItemOrOffensiveSpellBaseTestsSpell spell = new ItemOrOffensiveSpellBaseTestsSpell(randomManagerMock.Object, wiznetMock.Object);
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, "item", new CommandParameter("item", false));

            string result = spell.Guards(abilityActionInput);

            Assert.IsNull(result);
        }

        // Spell without specific guards nor invoke
        [Spell(SpellName, AbilityEffects.None)]
        internal class ItemOrOffensiveSpellBaseTestsSpell : ItemOrOffensiveSpellBase
        {
            public ItemOrOffensiveSpellBaseTestsSpell(IRandomManager randomManager, IWiznet wiznet)
                : base(randomManager, wiznet)
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
