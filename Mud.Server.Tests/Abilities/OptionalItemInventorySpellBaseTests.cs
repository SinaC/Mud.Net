using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    public class OptionalItemInventorySpellBaseTests : TestBase
    {
        private const string SpellName = "OptionalItemInventorySpellBaseTests_Spell";

        [TestMethod]
        public void Setup_ItemNotFound()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityLearnedInfo(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            OptionalItemInventorySpellBaseTestsSpell spell = new OptionalItemInventorySpellBaseTestsSpell(randomManagerMock.Object);

            var parameters = BuildParameters("item");
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, 10, null, parameters);

            string result = spell.Setup(abilityActionInput);

            Assert.AreEqual("You are not carrying that.", result);
        }

        [TestMethod]
        public void Setup_NoItem()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityLearnedInfo(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            OptionalItemInventorySpellBaseTestsSpell spell = new OptionalItemInventorySpellBaseTestsSpell(randomManagerMock.Object);

            var parameters = BuildParameters("");
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, 10, null, parameters);

            string result = spell.Setup(abilityActionInput);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Setup_ItemInEquipment()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IItem> itemMock = new Mock<IItem>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.SetupGet(x => x.Equipments).Returns(new EquippedItem(EquipmentSlots.Chest) { Item = itemMock.Object }.Yield());
            casterMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityLearnedInfo(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            itemMock.SetupGet(x => x.Name).Returns("item");
            itemMock.SetupGet(x => x.Keywords).Returns("item".Yield());
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            OptionalItemInventorySpellBaseTestsSpell spell = new OptionalItemInventorySpellBaseTestsSpell(randomManagerMock.Object);

            var parameters = BuildParameters("item");
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, 10, null, parameters);

            string result = spell.Setup(abilityActionInput);

            Assert.AreEqual("You are not carrying that.", result);
        }

        [TestMethod]
        public void Setup_ItemInRoom()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IItem> itemMock = new Mock<IItem>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityLearnedInfo(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            itemMock.SetupGet(x => x.Name).Returns("item");
            itemMock.SetupGet(x => x.Keywords).Returns("item".Yield());
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            roomMock.SetupGet(x => x.Content).Returns(itemMock.Object.Yield());
            OptionalItemInventorySpellBaseTestsSpell spell = new OptionalItemInventorySpellBaseTestsSpell(randomManagerMock.Object);

            var parameters = BuildParameters("item");
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, 10, null, parameters);

            string result = spell.Setup(abilityActionInput);

            Assert.AreEqual("You are not carrying that.", result);
        }

        [TestMethod]
        public void Setup_ItemSpecifiedAndFound()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IItem> itemMock = new Mock<IItem>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.SetupGet(x => x.Inventory).Returns(itemMock.Object.Yield());
            casterMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityLearnedInfo(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            itemMock.SetupGet(x => x.Name).Returns("item");
            itemMock.SetupGet(x => x.Keywords).Returns("item".Yield());
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            OptionalItemInventorySpellBaseTestsSpell spell = new OptionalItemInventorySpellBaseTestsSpell(randomManagerMock.Object);

            var parameters = BuildParameters("item");
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, 10, null, parameters);

            string result = spell.Setup(abilityActionInput);

            Assert.IsNull(result);
        }

        // Spell without specific Setup nor invoke
        [Spell(SpellName, AbilityEffects.None)]
        internal class OptionalItemInventorySpellBaseTestsSpell : OptionalItemInventorySpellBase
        {
            public OptionalItemInventorySpellBaseTestsSpell(IRandomManager randomManager)
                : base(randomManager)
            {
            }

            protected override void Invoke()
            {
            }
        }
    }
}
