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
    public class ItemInventorySpellBaseTests : TestBase
    {
        public const string SpellName = "ItemInventorySpellBaseTests_Spell";

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
            ItemInventorySpellBaseTestsSpell spell = new ItemInventorySpellBaseTestsSpell(randomManagerMock.Object, wiznetMock.Object);
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
            ItemInventorySpellBaseTestsSpell spell = new ItemInventorySpellBaseTestsSpell(randomManagerMock.Object, wiznetMock.Object);
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, "", Enumerable.Empty<CommandParameter>().ToArray());

            string result = spell.Guards(abilityActionInput);

            Assert.AreEqual("What should the spell be cast upon?", result);
        }

        [TestMethod]
        public void Guards_ItemWrongType()
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
            ItemInventorySpellBaseTestsSpell spell = new ItemInventorySpellBaseTestsSpell(randomManagerMock.Object, wiznetMock.Object);
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, "item", new CommandParameter("item", false));

            string result = spell.Guards(abilityActionInput);

            Assert.AreEqual("Ceci n'est pas une pipe", result);
        }

        [TestMethod]
        public void Guards_ItemInEquipment()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IWiznet> wiznetMock = new Mock<IWiznet>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IItemWeapon> itemMock = new Mock<IItemWeapon>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.SetupGet(x => x.Equipments).Returns(new EquippedItem(POC.Abilities2.Domain.EquipmentSlots.Chest) { Item = itemMock.Object }.Yield());
            casterMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityPercentage(It.IsAny<IAbility>())).Returns<IAbility>(x => (100, new AbilityLearned { Name = SpellName }));
            itemMock.SetupGet(x => x.Name).Returns("item");
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            ItemInventorySpellBaseTestsSpell spell = new ItemInventorySpellBaseTestsSpell(randomManagerMock.Object, wiznetMock.Object);
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
            Mock<IItemWeapon> itemMock = new Mock<IItemWeapon>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityPercentage(It.IsAny<IAbility>())).Returns<IAbility>(x => (100, new AbilityLearned { Name = SpellName }));
            itemMock.SetupGet(x => x.Name).Returns("item");
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            roomMock.SetupGet(x => x.Content).Returns(itemMock.Object.Yield());
            ItemInventorySpellBaseTestsSpell spell = new ItemInventorySpellBaseTestsSpell(randomManagerMock.Object, wiznetMock.Object);
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, "item", new CommandParameter("item", false));

            string result = spell.Guards(abilityActionInput);

            Assert.AreEqual("You are not carrying that.", result);
        }

        [TestMethod]
        public void Guards_ItemFoundInInventory()
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
            casterMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityPercentage(It.IsAny<IAbility>())).Returns<IAbility>(x => (100, new AbilityLearned { Name = SpellName }));
            itemMock.SetupGet(x => x.Name).Returns("item");
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            ItemInventorySpellBaseTestsSpell spell = new ItemInventorySpellBaseTestsSpell(randomManagerMock.Object, wiznetMock.Object);
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, "item", new CommandParameter("item", false));

            string result = spell.Guards(abilityActionInput);

            Assert.IsNull(result);
        }

        // Spell without specific guards nor invoke
        [Spell(SpellName, AbilityEffects.None)]
        internal class ItemInventorySpellBaseTestsSpell : ItemInventorySpellBase<IItemWeapon>
        {
            public ItemInventorySpellBaseTestsSpell(IRandomManager randomManager, IWiznet wiznet)
                : base(randomManager, wiznet)
            {
            }

            protected override void Invoke()
            {
            }

            protected override string InvalidItemTypeMsg => "Ceci n'est pas une pipe";
        }
    }
}
