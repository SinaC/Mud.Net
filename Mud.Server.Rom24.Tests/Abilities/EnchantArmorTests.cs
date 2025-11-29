using Microsoft.Extensions.Logging;
using Moq;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Flags;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Random;
using Mud.Server.Rom24.Spells;

namespace Mud.Server.Rom24.Tests.Abilities
{
    [TestClass]
    public class EnchantArmorTests : AbilityTestBase
    {
        [TestMethod]
        public void ItemDestroyed()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            EnchantArmor spell = new EnchantArmor(new Mock<ILogger<EnchantArmor>>().Object, _itemFlagFactory, randomManagerMock.Object, auraManagerMock.Object, itemManagerMock.Object);

            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            Mock<IItemArmor> armorMock = new Mock<IItemArmor>();
            casterMock.SetupGet(x => x.Level).Returns(1);
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.SetupGet(x => x.Inventory).Returns(armorMock.Object.Yield());
            casterMock.Setup(x => x.CanSee(armorMock.Object)).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityLearnedInfo(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            armorMock.SetupGet(x => x.Name).Returns("armor");
            armorMock.SetupGet(x => x.Keywords).Returns("armor".Yield());
            armorMock.SetupGet(x => x.ItemFlags).Returns(_itemFlagFactory.CreateInstance());

            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns(true);
            randomManagerMock.Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>())).Returns<int, int>((min, max) => 0); // must be below 25/5

            var parameters = BuildParameters("armor");
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(new Mock<ILogger>().Object, spell.GetType()), casterMock.Object, 0, null, parameters);

            var result = spell.Setup(abilityActionInput);
            spell.Execute();

            Assert.IsNull(result);
            itemManagerMock.Verify(x => x.RemoveItem(armorMock.Object), Times.Once);
            armorMock.Verify(x => x.Disenchant(), Times.Never);
            armorMock.Verify(x => x.IncreaseLevel(), Times.Never);
            auraManagerMock.Verify(x => x.AddAura(It.IsAny<IEntity>(), It.IsAny<string>(), It.IsAny<ICharacter>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Never);
        }

        [TestMethod]
        public void ItemDisenchanted()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            EnchantArmor spell = new EnchantArmor(new Mock<ILogger<EnchantArmor>>().Object, _itemFlagFactory, randomManagerMock.Object, auraManagerMock.Object, itemManagerMock.Object);

            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            Mock<IItemArmor> armorMock = new Mock<IItemArmor>();
            casterMock.SetupGet(x => x.Level).Returns(1);
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.SetupGet(x => x.Inventory).Returns(armorMock.Object.Yield());
            casterMock.Setup(x => x.CanSee(armorMock.Object)).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityLearnedInfo(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            armorMock.SetupGet(x => x.Name).Returns("armor");
            armorMock.SetupGet(x => x.Keywords).Returns("armor".Yield());
            armorMock.SetupGet(x => x.ItemFlags).Returns(_itemFlagFactory.CreateInstance());

            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns(true);
            randomManagerMock.Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>())).Returns<int, int>((min, max) => 7); // must be between 25/5 and 25/3

            var parameters = BuildParameters("armor");
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(new Mock<ILogger>().Object, spell.GetType()), casterMock.Object, 0, null, parameters);

            var result = spell.Setup(abilityActionInput);
            spell.Execute();

            Assert.IsNull(result);
            itemManagerMock.Verify(x => x.RemoveItem(armorMock.Object), Times.Never);
            armorMock.Verify(x => x.Disenchant(), Times.Once);
            armorMock.Verify(x => x.IncreaseLevel(), Times.Never);
            auraManagerMock.Verify(x => x.AddAura(It.IsAny<IEntity>(), It.IsAny<string>(), It.IsAny<ICharacter>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Never);
        }

        [TestMethod]
        public void NothingHappened()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            EnchantArmor spell = new EnchantArmor(new Mock<ILogger<EnchantArmor>>().Object, _itemFlagFactory, randomManagerMock.Object, auraManagerMock.Object, itemManagerMock.Object);

            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            Mock<IItemArmor> armorMock = new Mock<IItemArmor>();
            casterMock.SetupGet(x => x.Level).Returns(1);
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.SetupGet(x => x.Inventory).Returns(armorMock.Object.Yield());
            casterMock.Setup(x => x.CanSee(armorMock.Object)).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityLearnedInfo(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            armorMock.SetupGet(x => x.Name).Returns("armor");
            armorMock.SetupGet(x => x.Keywords).Returns("armor".Yield());
            armorMock.SetupGet(x => x.ItemFlags).Returns(_itemFlagFactory.CreateInstance());

            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns(true);
            randomManagerMock.Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>())).Returns<int, int>((min, max) => 20); // must be between 25/3 and 25

            var parameters = BuildParameters("armor");
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(new Mock<ILogger>().Object, spell.GetType()), casterMock.Object, 0, null, parameters);

            var result = spell.Setup(abilityActionInput);
            spell.Execute();

            Assert.IsNull(result);
            itemManagerMock.Verify(x => x.RemoveItem(armorMock.Object), Times.Never);
            armorMock.Verify(x => x.Disenchant(), Times.Never);
            casterMock.Verify(x => x.Send("Nothing seemed to happen."), Times.Once);
            armorMock.Verify(x => x.IncreaseLevel(), Times.Never);
            auraManagerMock.Verify(x => x.AddAura(It.IsAny<IEntity>(), It.IsAny<string>(), It.IsAny<ICharacter>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Never);
        }

        [TestMethod]
        public void NormalEnchant()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            EnchantArmor spell = new EnchantArmor(new Mock<ILogger<EnchantArmor>>().Object, _itemFlagFactory, randomManagerMock.Object, auraManagerMock.Object, itemManagerMock.Object);

            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            Mock<IItemArmor> armorMock = new Mock<IItemArmor>();
            casterMock.SetupGet(x => x.Level).Returns(1);
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.SetupGet(x => x.Inventory).Returns(armorMock.Object.Yield());
            casterMock.Setup(x => x.CanSee(armorMock.Object)).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityLearnedInfo(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            armorMock.SetupGet(x => x.Name).Returns("armor");
            armorMock.SetupGet(x => x.Keywords).Returns("armor".Yield());
            armorMock.SetupGet(x => x.ItemFlags).Returns(_itemFlagFactory.CreateInstance());

            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns(true);
            randomManagerMock.Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>())).Returns<int, int>((min, max) => 50); // must be between 25 and 90

            var parameters = BuildParameters("armor");
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(new Mock<ILogger>().Object, spell.GetType()), casterMock.Object, 0, null, parameters);

            var result = spell.Setup(abilityActionInput);
            spell.Execute();

            Assert.IsNull(result);
            itemManagerMock.Verify(x => x.RemoveItem(armorMock.Object), Times.Never);
            armorMock.Verify(x => x.Disenchant(), Times.Never);
            armorMock.Verify(x => x.IncreaseLevel(), Times.Once);
            casterMock.Verify(x => x.Act(It.IsAny<ActOptions>(), "{0} shimmers with a gold aura.", It.IsAny<object[]>()), Times.Once);
            auraManagerMock.Verify(x => x.AddAura(It.IsAny<IEntity>(), It.IsAny<string>(), It.IsAny<ICharacter>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
        }

        [TestMethod]
        public void ExceptionalEnchant()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            EnchantArmor spell = new EnchantArmor(new Mock<ILogger<EnchantArmor>>().Object, _itemFlagFactory, randomManagerMock.Object, auraManagerMock.Object, itemManagerMock.Object);

            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            Mock<IItemArmor> armorMock = new Mock<IItemArmor>();
            casterMock.SetupGet(x => x.Level).Returns(1);
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.SetupGet(x => x.Inventory).Returns(armorMock.Object.Yield());
            casterMock.Setup(x => x.CanSee(armorMock.Object)).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityLearnedInfo(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            armorMock.SetupGet(x => x.Name).Returns("armor");
            armorMock.SetupGet(x => x.Keywords).Returns("armor".Yield());
            armorMock.SetupGet(x => x.ItemFlags).Returns(_itemFlagFactory.CreateInstance());

            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns(true);
            randomManagerMock.Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>())).Returns<int, int>((min, max) => 100); // must be greater than 90

            var parameters = BuildParameters("armor");
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(new Mock<ILogger>().Object, spell.GetType()), casterMock.Object, 0, null, parameters);

            var result = spell.Setup(abilityActionInput);
            spell.Execute();

            Assert.IsNull(result);
            itemManagerMock.Verify(x => x.RemoveItem(armorMock.Object), Times.Never);
            armorMock.Verify(x => x.Disenchant(), Times.Never);
            armorMock.Verify(x => x.IncreaseLevel(), Times.Once);
            casterMock.Verify(x => x.Act(It.IsAny<ActOptions>(), "{0} glows a brillant gold!", It.IsAny<object[]>()), Times.Once);
            auraManagerMock.Verify(x => x.AddAura(It.IsAny<IEntity>(), It.IsAny<string>(), It.IsAny<ICharacter>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
        }
    }
}
