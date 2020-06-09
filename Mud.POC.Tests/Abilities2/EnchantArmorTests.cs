using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.POC.Abilities2;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Rom24Spells;
using Mud.Server.Common;
using System;

namespace Mud.POC.Tests.Abilities2
{
    [TestClass]
    public class EnchantArmorTests : TestBase
    {
        [TestMethod]
        public void ItemDestroyed()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            EnchantArmor spell = new EnchantArmor(randomManagerMock.Object, auraManagerMock.Object, itemManagerMock.Object);

            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            Mock<IItemArmor> armorMock = new Mock<IItemArmor>();
            casterMock.SetupGet(x => x.Level).Returns(1);
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.SetupGet(x => x.Inventory).Returns(armorMock.Object.Yield());
            casterMock.Setup(x => x.CanSee(armorMock.Object)).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(_ => (100, new AbilityLearned { Name = "Enchant Armor" }));
            armorMock.SetupGet(x => x.Name).Returns("armor");

            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns(true);
            randomManagerMock.Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>())).Returns<int, int>((min, max) => 0); // must be below 25/5

            var parameters = BuildParameters("armor");
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, parameters.rawParameters, parameters.parameters);

            string result = spell.Setup(abilityActionInput);
            spell.Execute();

            Assert.IsNull(result);
            itemManagerMock.Verify(x => x.RemoveItem(armorMock.Object), Times.Once);
            armorMock.Verify(x => x.Disenchant(), Times.Never);
            armorMock.Verify(x => x.IncreaseLevel(), Times.Never);
            auraManagerMock.Verify(x => x.AddAura(It.IsAny<IEntity>(), It.IsAny<string>(), It.IsAny<ICharacter>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<POC.Abilities2.Domain.AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Never);
        }

        [TestMethod]
        public void ItemDisenchanted()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            EnchantArmor spell = new EnchantArmor(randomManagerMock.Object, auraManagerMock.Object, itemManagerMock.Object);

            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            Mock<IItemArmor> armorMock = new Mock<IItemArmor>();
            casterMock.SetupGet(x => x.Level).Returns(1);
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.SetupGet(x => x.Inventory).Returns(armorMock.Object.Yield());
            casterMock.Setup(x => x.CanSee(armorMock.Object)).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(_ => (100, new AbilityLearned { Name = "Enchant Armor" }));
            armorMock.SetupGet(x => x.Name).Returns("armor");

            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns(true);
            randomManagerMock.Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>())).Returns<int, int>((min, max) => 7); // must be between 25/5 and 25/3

            var parameters = BuildParameters("armor");
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, parameters.rawParameters, parameters.parameters);

            string result = spell.Setup(abilityActionInput);
            spell.Execute();

            Assert.IsNull(result);
            itemManagerMock.Verify(x => x.RemoveItem(armorMock.Object), Times.Never);
            armorMock.Verify(x => x.Disenchant(), Times.Once);
            armorMock.Verify(x => x.IncreaseLevel(), Times.Never);
            auraManagerMock.Verify(x => x.AddAura(It.IsAny<IEntity>(), It.IsAny<string>(), It.IsAny<ICharacter>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<POC.Abilities2.Domain.AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Never);
        }

        [TestMethod]
        public void NothingHappened()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            EnchantArmor spell = new EnchantArmor(randomManagerMock.Object, auraManagerMock.Object, itemManagerMock.Object);

            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            Mock<IItemArmor> armorMock = new Mock<IItemArmor>();
            casterMock.SetupGet(x => x.Level).Returns(1);
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.SetupGet(x => x.Inventory).Returns(armorMock.Object.Yield());
            casterMock.Setup(x => x.CanSee(armorMock.Object)).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(_ => (100, new AbilityLearned { Name = "Enchant Armor" }));
            armorMock.SetupGet(x => x.Name).Returns("armor");

            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns(true);
            randomManagerMock.Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>())).Returns<int, int>((min, max) => 20); // must be between 25/3 and 25

            var parameters = BuildParameters("armor");
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, parameters.rawParameters, parameters.parameters);

            string result = spell.Setup(abilityActionInput);
            spell.Execute();

            Assert.IsNull(result);
            itemManagerMock.Verify(x => x.RemoveItem(armorMock.Object), Times.Never);
            armorMock.Verify(x => x.Disenchant(), Times.Never);
            casterMock.Verify(x => x.Send("Nothing seemed to happen."), Times.Once);
            armorMock.Verify(x => x.IncreaseLevel(), Times.Never);
            auraManagerMock.Verify(x => x.AddAura(It.IsAny<IEntity>(), It.IsAny<string>(), It.IsAny<ICharacter>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<POC.Abilities2.Domain.AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Never);
        }

        [TestMethod]
        public void NormalEnchant()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            EnchantArmor spell = new EnchantArmor(randomManagerMock.Object, auraManagerMock.Object, itemManagerMock.Object);

            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            Mock<IItemArmor> armorMock = new Mock<IItemArmor>();
            casterMock.SetupGet(x => x.Level).Returns(1);
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.SetupGet(x => x.Inventory).Returns(armorMock.Object.Yield());
            casterMock.Setup(x => x.CanSee(armorMock.Object)).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(_ => (100, new AbilityLearned { Name = "Enchant Armor" }));
            armorMock.SetupGet(x => x.Name).Returns("armor");

            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns(true);
            randomManagerMock.Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>())).Returns<int, int>((min, max) => 50); // must be between 25 and 90

            var parameters = BuildParameters("armor");
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, parameters.rawParameters, parameters.parameters);

            string result = spell.Setup(abilityActionInput);
            spell.Execute();

            Assert.IsNull(result);
            itemManagerMock.Verify(x => x.RemoveItem(armorMock.Object), Times.Never);
            armorMock.Verify(x => x.Disenchant(), Times.Never);
            armorMock.Verify(x => x.IncreaseLevel(), Times.Once);
            casterMock.Verify(x => x.Act(It.IsAny<ActOptions>(), "{0} shimmers with a gold aura.", It.IsAny<object[]>()), Times.Once);
            auraManagerMock.Verify(x => x.AddAura(It.IsAny<IEntity>(), It.IsAny<string>(), It.IsAny<ICharacter>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<POC.Abilities2.Domain.AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
        }

        [TestMethod]
        public void ExceptionalEnchant()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            EnchantArmor spell = new EnchantArmor(randomManagerMock.Object, auraManagerMock.Object, itemManagerMock.Object);

            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            Mock<IItemArmor> armorMock = new Mock<IItemArmor>();
            casterMock.SetupGet(x => x.Level).Returns(1);
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.SetupGet(x => x.Inventory).Returns(armorMock.Object.Yield());
            casterMock.Setup(x => x.CanSee(armorMock.Object)).Returns<IItem>(_ => true);
            casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(_ => (100, new AbilityLearned { Name = "Enchant Armor" }));
            armorMock.SetupGet(x => x.Name).Returns("armor");

            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns(true);
            randomManagerMock.Setup(x => x.Range(It.IsAny<int>(), It.IsAny<int>())).Returns<int, int>((min, max) => 100); // must be greater than 90

            var parameters = BuildParameters("armor");
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, parameters.rawParameters, parameters.parameters);

            string result = spell.Setup(abilityActionInput);
            spell.Execute();

            Assert.IsNull(result);
            itemManagerMock.Verify(x => x.RemoveItem(armorMock.Object), Times.Never);
            armorMock.Verify(x => x.Disenchant(), Times.Never);
            armorMock.Verify(x => x.IncreaseLevel(), Times.Once);
            casterMock.Verify(x => x.Act(It.IsAny<ActOptions>(), "{0} glows a brillant gold!", It.IsAny<object[]>()), Times.Once);
            auraManagerMock.Verify(x => x.AddAura(It.IsAny<IEntity>(), It.IsAny<string>(), It.IsAny<ICharacter>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<POC.Abilities2.Domain.AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
        }
    }
}
