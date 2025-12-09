using Microsoft.Extensions.Logging;
using Moq;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Character;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Random;

namespace Mud.Server.Tests.Abilities
{
    [TestClass]
    public class ItemInventorySpellBaseTests : AbilityTestBase
    {
        public const string SpellName = "ItemInventorySpellBaseTests_Spell";

        [TestMethod]
        public void Setup_ItemNotFound()
        {
            Mock<IRandomManager> randomManagerMock = new();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new();
            Mock<IPlayableCharacter> casterMock = new();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            ItemInventorySpellBaseTestsSpell spell = new(new Mock<ILogger<ItemInventorySpellBaseTestsSpell>>().Object, randomManagerMock.Object);

            var parameters = BuildParameters("item");
            SpellActionInput abilityActionInput = new(new AbilityDefinition( spell.GetType()), casterMock.Object, 10, null, parameters);

            var result = spell.Setup(abilityActionInput);

            Assert.AreEqual("You are not carrying that.", result);
        }

        [TestMethod]
        public void Setup_NoItem()
        {
            Mock<IRandomManager> randomManagerMock = new();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new();
            Mock<IPlayableCharacter> casterMock = new();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            ItemInventorySpellBaseTestsSpell spell = new(new Mock<ILogger<ItemInventorySpellBaseTestsSpell>>().Object, randomManagerMock.Object);

            var parameters = BuildParameters("");
            SpellActionInput abilityActionInput = new(new AbilityDefinition( spell.GetType()), casterMock.Object, 10, null, parameters);

            var result = spell.Setup(abilityActionInput);

            Assert.AreEqual("What should the spell be cast upon?", result);
        }

        [TestMethod]
        public void Setup_ItemWrongType()
        {
            Mock<IRandomManager> randomManagerMock = new();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new();
            Mock<IItem> itemMock = new();
            Mock<IPlayableCharacter> casterMock = new();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.SetupGet(x => x.Inventory).Returns(itemMock.Object.Yield());
            casterMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            itemMock.SetupGet(x => x.Name).Returns("item");
            itemMock.SetupGet(x => x.Keywords).Returns("item".Yield());
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            ItemInventorySpellBaseTestsSpell spell = new(new Mock<ILogger<ItemInventorySpellBaseTestsSpell>>().Object, randomManagerMock.Object);

            var parameters = BuildParameters("item");
            SpellActionInput abilityActionInput = new(new AbilityDefinition( spell.GetType()), casterMock.Object, 10, null, parameters);

            var result = spell.Setup(abilityActionInput);

            Assert.AreEqual("Ceci n'est pas une pipe", result);
        }

        [TestMethod]
        public void Setup_ItemInEquipment()
        {
            Mock<IRandomManager> randomManagerMock = new();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new();
            Mock<IItemWeapon> itemMock = new();
            Mock<IPlayableCharacter> casterMock = new();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.SetupGet(x => x.Equipments).Returns(() => new EquippedItem(new Mock<ILogger>().Object, EquipmentSlots.Chest) { Item = itemMock.Object }.Yield());
            casterMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            itemMock.SetupGet(x => x.Name).Returns("item");
            itemMock.SetupGet(x => x.Keywords).Returns("item".Yield());
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            ItemInventorySpellBaseTestsSpell spell = new(new Mock<ILogger<ItemInventorySpellBaseTestsSpell>>().Object, randomManagerMock.Object);

            var parameters = BuildParameters("item");
            SpellActionInput abilityActionInput = new(new AbilityDefinition( spell.GetType()), casterMock.Object, 10, null, parameters);

            var result = spell.Setup(abilityActionInput);

            Assert.AreEqual("You are not carrying that.", result);
        }

        [TestMethod]
        public void Setup_ItemInRoom()
        {
            Mock<IRandomManager> randomManagerMock = new();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new();
            Mock<IItemWeapon> itemMock = new();
            Mock<IPlayableCharacter> casterMock = new();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            itemMock.SetupGet(x => x.Name).Returns("item");
            itemMock.SetupGet(x => x.Keywords).Returns("item".Yield());
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            roomMock.SetupGet(x => x.Content).Returns(itemMock.Object.Yield());
            ItemInventorySpellBaseTestsSpell spell = new(new Mock<ILogger<ItemInventorySpellBaseTestsSpell>>().Object, randomManagerMock.Object);

            var parameters = BuildParameters("item");
            SpellActionInput abilityActionInput = new(new AbilityDefinition( spell.GetType()), casterMock.Object, 10, null, parameters);

            var result = spell.Setup(abilityActionInput);

            Assert.AreEqual("You are not carrying that.", result);
        }

        [TestMethod]
        public void Setup_ItemFoundInInventory()
        {
            Mock<IRandomManager> randomManagerMock = new();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new();
            Mock<IItemWeapon> itemMock = new();
            Mock<IPlayableCharacter> casterMock = new();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.SetupGet(x => x.Inventory).Returns(itemMock.Object.Yield());
            casterMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            itemMock.SetupGet(x => x.Name).Returns("item");
            itemMock.SetupGet(x => x.Keywords).Returns("item".Yield());
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            ItemInventorySpellBaseTestsSpell spell = new(new Mock<ILogger<ItemInventorySpellBaseTestsSpell>>().Object, randomManagerMock.Object);

            var parameters = BuildParameters("item");
            SpellActionInput abilityActionInput = new(new AbilityDefinition( spell.GetType()), casterMock.Object, 10, null, parameters);

            var result = spell.Setup(abilityActionInput);

            Assert.IsNull(result);
        }

        // Spell without specific Setup nor invoke
        [Spell(SpellName, AbilityEffects.None)]
        public class ItemInventorySpellBaseTestsSpell : ItemInventorySpellBase<IItemWeapon>
        {
            public ItemInventorySpellBaseTestsSpell(ILogger<ItemInventorySpellBaseTestsSpell> logger, IRandomManager randomManager)
                : base(logger, randomManager)
            {
            }

            protected override void Invoke()
            {
            }

            protected override string InvalidItemTypeMsg => "Ceci n'est pas une pipe";
        }
    }
}
