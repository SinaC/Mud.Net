﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Flags;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Random;
using Mud.Server.Rom24.Skills;
using Mud.Server.Rom24.Spells;

namespace Mud.Server.Tests.Abilities
{
    [TestClass]
    public class StavesTests : TestBase
    {
        // set 2 characters in room, 2 items in inventory, 2 items in room

        // no target spell -> invalid spell on staves
        [TestMethod]
        public void NoTargetSpell()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Earthquake", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(typeof(Earthquake)));
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Earthquake(randomManagerMock.Object));

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IItem> inventoryItem1Mock = new Mock<IItem>();
            Mock<IItem> inventoryItem2Mock = new Mock<IItem>();
            Mock<IItem> roomItem1Mock = new Mock<IItem>();
            Mock<IItem> roomItem2Mock = new Mock<IItem>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            Mock<ICharacter> victimMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            victimMock.SetupGet(x => x.Name).Returns("victim");
            victimMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            victimMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            victimMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            victimMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
            victimMock.Setup(x => x.Inventory).Returns(new IItem[] { inventoryItem1Mock.Object, inventoryItem2Mock.Object });
            inventoryItem1Mock.SetupGet(x => x.Name).Returns("inventoryitem1");
            inventoryItem1Mock.SetupGet(x => x.ItemFlags).Returns(new ItemFlags());
            inventoryItem2Mock.SetupGet(x => x.Name).Returns("inventoryitem2");
            inventoryItem2Mock.SetupGet(x => x.ItemFlags).Returns(new ItemFlags());
            roomItem1Mock.SetupGet(x => x.Name).Returns("roomItem1");
            roomItem1Mock.SetupGet(x => x.ItemFlags).Returns(new ItemFlags());
            roomItem2Mock.SetupGet(x => x.Name).Returns("roomItem2");
            roomItem2Mock.SetupGet(x => x.ItemFlags).Returns(new ItemFlags());
            roomMock.SetupGet(x => x.People).Returns( new ICharacter[] { userMock.Object, victimMock.Object });
            roomMock.SetupGet(x => x.Content).Returns(new IItem[] { roomItem1Mock.Object, roomItem2Mock.Object });
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemStaff> staffMock = new Mock<IItemStaff>();
            staffMock.SetupGet(x => x.Name).Returns("staff");
            staffMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            staffMock.SetupGet(x => x.SpellName).Returns("Earthquake");
            userMock.Setup(x => x.GetEquipment<IItemStaff>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => staffMock.Object);

            Staves skill = new Staves(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Staves>(userMock.Object, "brandish");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);

            Assert.AreEqual("Something goes wrong.", result);
        }

        // character offensive target spell -> cast on each target
        [TestMethod]
        public void CharacterOffensiveTargetSpell()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Fireball", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(typeof(Fireball)));
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Fireball(randomManagerMock.Object));

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IItem> inventoryItem1Mock = new Mock<IItem>();
            Mock<IItem> inventoryItem2Mock = new Mock<IItem>();
            Mock<IItem> roomItem1Mock = new Mock<IItem>();
            Mock<IItem> roomItem2Mock = new Mock<IItem>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            Mock<ICharacter> victimMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            userMock.Setup(x => x.Inventory).Returns(new IItem[] { inventoryItem1Mock.Object, inventoryItem2Mock.Object });
            victimMock.SetupGet(x => x.Name).Returns("victim");
            victimMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            victimMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            victimMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            victimMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
            inventoryItem1Mock.SetupGet(x => x.Name).Returns("inventoryitem1");
            inventoryItem1Mock.SetupGet(x => x.ItemFlags).Returns(new ItemFlags());
            inventoryItem2Mock.SetupGet(x => x.Name).Returns("inventoryitem2");
            inventoryItem2Mock.SetupGet(x => x.ItemFlags).Returns(new ItemFlags());
            roomItem1Mock.SetupGet(x => x.Name).Returns("roomItem1");
            roomItem1Mock.SetupGet(x => x.ItemFlags).Returns(new ItemFlags());
            roomItem2Mock.SetupGet(x => x.Name).Returns("roomItem2");
            roomItem2Mock.SetupGet(x => x.ItemFlags).Returns(new ItemFlags());
            roomMock.SetupGet(x => x.People).Returns(new ICharacter[] { userMock.Object, victimMock.Object });
            roomMock.SetupGet(x => x.Content).Returns(new IItem[] { roomItem1Mock.Object, roomItem2Mock.Object });
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemStaff> staffMock = new Mock<IItemStaff>();
            staffMock.SetupGet(x => x.Name).Returns("staff");
            staffMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            staffMock.SetupGet(x => x.SpellName).Returns("Fireball");
            userMock.Setup(x => x.GetEquipment<IItemStaff>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => staffMock.Object);

            Staves skill = new Staves(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Staves>(userMock.Object, "brandish");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);
            skill.Execute();

            Assert.IsNull(result);
            userMock.Verify(x => x.AbilityDamage(userMock.Object, It.IsAny<int>(), SchoolTypes.Fire, "fireball", It.IsAny<bool>()), Times.Never);
            victimMock.Verify(x => x.AbilityDamage(userMock.Object, It.IsAny<int>(), SchoolTypes.Fire, "fireball", It.IsAny<bool>()), Times.Once);
        }

        // character offensive target spell -> cast on each target
        [TestMethod]
        public void CharacterDefensiveTargetSpell()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Armor", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(typeof(Armor)));
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Armor(randomManagerMock.Object, auraManagerMock.Object));

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IItem> inventoryItem1Mock = new Mock<IItem>();
            Mock<IItem> inventoryItem2Mock = new Mock<IItem>();
            Mock<IItem> roomItem1Mock = new Mock<IItem>();
            Mock<IItem> roomItem2Mock = new Mock<IItem>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            Mock<ICharacter> victimMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            userMock.Setup(x => x.Inventory).Returns(new IItem[] { inventoryItem1Mock.Object, inventoryItem2Mock.Object });
            victimMock.SetupGet(x => x.Name).Returns("victim");
            victimMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            victimMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            victimMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            victimMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
            inventoryItem1Mock.SetupGet(x => x.Name).Returns("inventoryitem1");
            inventoryItem1Mock.SetupGet(x => x.ItemFlags).Returns(new ItemFlags());
            inventoryItem2Mock.SetupGet(x => x.Name).Returns("inventoryitem2");
            inventoryItem2Mock.SetupGet(x => x.ItemFlags).Returns(new ItemFlags());
            roomItem1Mock.SetupGet(x => x.Name).Returns("roomItem1");
            roomItem1Mock.SetupGet(x => x.ItemFlags).Returns(new ItemFlags());
            roomItem2Mock.SetupGet(x => x.Name).Returns("roomItem2");
            roomItem2Mock.SetupGet(x => x.ItemFlags).Returns(new ItemFlags());
            roomMock.SetupGet(x => x.People).Returns(new ICharacter[] { userMock.Object, victimMock.Object });
            roomMock.SetupGet(x => x.Content).Returns(new IItem[] { roomItem1Mock.Object, roomItem2Mock.Object });
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemStaff> staffMock = new Mock<IItemStaff>();
            staffMock.SetupGet(x => x.Name).Returns("staff");
            staffMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            staffMock.SetupGet(x => x.SpellName).Returns("Armor");
            userMock.Setup(x => x.GetEquipment<IItemStaff>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => staffMock.Object);

            Staves skill = new Staves(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Staves>(userMock.Object, "brandish");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);
            skill.Execute();

            Assert.IsNull(result);
            userMock.Verify(x => x.Act(ActOptions.ToCharacter, "You feel someone protecting you.", It.IsAny<object[]>()), Times.Once);
            victimMock.Verify(x => x.Act(ActOptions.ToCharacter, "You feel someone protecting you.", It.IsAny<object[]>()), Times.Once);
        }

        // item target spell -> cast on each item in inventory
        [TestMethod]
        public void ItemTargetSpell()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Fireproof", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(typeof(Fireproof)));
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Fireproof(randomManagerMock.Object, auraManagerMock.Object));

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IItem> inventoryItem1Mock = new Mock<IItem>();
            Mock<IItem> inventoryItem2Mock = new Mock<IItem>();
            Mock<IItem> roomItem1Mock = new Mock<IItem>();
            Mock<IItem> roomItem2Mock = new Mock<IItem>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            Mock<ICharacter> victimMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            userMock.Setup(x => x.Inventory).Returns(new IItem[] { inventoryItem1Mock.Object, inventoryItem2Mock.Object });
            victimMock.SetupGet(x => x.Name).Returns("victim");
            victimMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            victimMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            victimMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            victimMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
            inventoryItem1Mock.SetupGet(x => x.Name).Returns("inventoryitem1");
            inventoryItem1Mock.SetupGet(x => x.ItemFlags).Returns(new ItemFlags());
            inventoryItem2Mock.SetupGet(x => x.Name).Returns("inventoryitem2");
            inventoryItem2Mock.SetupGet(x => x.ItemFlags).Returns(new ItemFlags());
            roomItem1Mock.SetupGet(x => x.Name).Returns("roomItem1");
            roomItem1Mock.SetupGet(x => x.ItemFlags).Returns(new ItemFlags());
            roomItem2Mock.SetupGet(x => x.Name).Returns("roomItem2");
            roomItem2Mock.SetupGet(x => x.ItemFlags).Returns(new ItemFlags());
            roomMock.SetupGet(x => x.People).Returns(new ICharacter[] { userMock.Object, victimMock.Object });
            roomMock.SetupGet(x => x.Content).Returns(new IItem[] { roomItem1Mock.Object, roomItem2Mock.Object });
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemStaff> staffMock = new Mock<IItemStaff>();
            staffMock.SetupGet(x => x.Name).Returns("staff");
            staffMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            staffMock.SetupGet(x => x.SpellName).Returns("Fireproof");
            userMock.Setup(x => x.GetEquipment<IItemStaff>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => staffMock.Object);

            Staves skill = new Staves(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Staves>(userMock.Object, "brandish");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);
            skill.Execute();

            Assert.IsNull(result);
            auraManagerMock.Verify(x => x.AddAura(inventoryItem1Mock.Object, "Fireproof", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
            auraManagerMock.Verify(x => x.AddAura(inventoryItem2Mock.Object, "Fireproof", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
            userMock.Verify(x => x.Act(ActOptions.ToCharacter, "You protect {0:N} from fire.", It.IsAny<object[]>()), Times.Exactly(2));
        }

        // mixed offensive target spell -> victim, items in inventory and items on room
        [TestMethod]
        public void MixedOffensiveTargetSpell()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            Mock<IDispelManager> dispelManagerMock = new Mock<IDispelManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Curse", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(typeof(Curse)));
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Curse(randomManagerMock.Object, auraManagerMock.Object, dispelManagerMock.Object));

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IItem> inventoryItem1Mock = new Mock<IItem>();
            Mock<IItem> inventoryItem2Mock = new Mock<IItem>();
            Mock<IItem> roomItem1Mock = new Mock<IItem>();
            Mock<IItem> roomItem2Mock = new Mock<IItem>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            Mock<ICharacter> victimMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            userMock.Setup(x => x.Inventory).Returns(new IItem[] { inventoryItem1Mock.Object, inventoryItem2Mock.Object });
            victimMock.SetupGet(x => x.Name).Returns("victim");
            victimMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            victimMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
            victimMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            victimMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            inventoryItem1Mock.SetupGet(x => x.Name).Returns("inventoryitem1");
            inventoryItem1Mock.SetupGet(x => x.ItemFlags).Returns(new ItemFlags());
            inventoryItem2Mock.SetupGet(x => x.Name).Returns("inventoryitem2");
            inventoryItem2Mock.SetupGet(x => x.ItemFlags).Returns(new ItemFlags());
            roomItem1Mock.SetupGet(x => x.Name).Returns("roomItem1");
            roomItem1Mock.SetupGet(x => x.ItemFlags).Returns(new ItemFlags());
            roomItem2Mock.SetupGet(x => x.Name).Returns("roomItem2");
            roomItem2Mock.SetupGet(x => x.ItemFlags).Returns(new ItemFlags());
            roomMock.SetupGet(x => x.People).Returns(new ICharacter[] { userMock.Object, victimMock.Object });
            roomMock.SetupGet(x => x.Content).Returns(new IItem[] { roomItem1Mock.Object, roomItem2Mock.Object });
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemStaff> staffMock = new Mock<IItemStaff>();
            staffMock.SetupGet(x => x.Name).Returns("staff");
            staffMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            staffMock.SetupGet(x => x.SpellName).Returns("Curse");
            userMock.Setup(x => x.GetEquipment<IItemStaff>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => staffMock.Object);

            Staves skill = new Staves(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Staves>(userMock.Object, "brandish");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);
            skill.Execute();

            Assert.IsNull(result);
            auraManagerMock.Verify(x => x.AddAura(inventoryItem1Mock.Object, "Curse", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
            auraManagerMock.Verify(x => x.AddAura(inventoryItem2Mock.Object, "Curse", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
            auraManagerMock.Verify(x => x.AddAura(userMock.Object, "Curse", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Never);
            auraManagerMock.Verify(x => x.AddAura(victimMock.Object, "Curse", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
            auraManagerMock.Verify(x => x.AddAura(roomItem1Mock.Object, "Curse", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
            auraManagerMock.Verify(x => x.AddAura(roomItem1Mock.Object, "Curse", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
        }

        // mixed defensive target spell -> user, victim, items in inventory
        [TestMethod]
        public void MixedDefensiveTargetSpell()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            Mock<IDispelManager> dispelManagerMock = new Mock<IDispelManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Invisibility", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(typeof(Invisibility)));
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Invisibility(randomManagerMock.Object, auraManagerMock.Object));

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IItem> inventoryItem1Mock = new Mock<IItem>();
            Mock<IItem> inventoryItem2Mock = new Mock<IItem>();
            Mock<IItem> roomItem1Mock = new Mock<IItem>();
            Mock<IItem> roomItem2Mock = new Mock<IItem>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            Mock<ICharacter> victimMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            userMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
            userMock.Setup(x => x.Inventory).Returns(new IItem[] { inventoryItem1Mock.Object, inventoryItem2Mock.Object });
            victimMock.SetupGet(x => x.Name).Returns("victim");
            victimMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            victimMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
            victimMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            victimMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            inventoryItem1Mock.SetupGet(x => x.Name).Returns("inventoryitem1");
            inventoryItem1Mock.SetupGet(x => x.ItemFlags).Returns(new ItemFlags());
            inventoryItem2Mock.SetupGet(x => x.Name).Returns("inventoryitem2");
            inventoryItem2Mock.SetupGet(x => x.ItemFlags).Returns(new ItemFlags());
            roomItem1Mock.SetupGet(x => x.Name).Returns("roomItem1");
            roomItem1Mock.SetupGet(x => x.ItemFlags).Returns(new ItemFlags());
            roomItem2Mock.SetupGet(x => x.Name).Returns("roomItem2");
            roomItem2Mock.SetupGet(x => x.ItemFlags).Returns(new ItemFlags());
            roomMock.SetupGet(x => x.People).Returns(new ICharacter[] { userMock.Object, victimMock.Object });
            roomMock.SetupGet(x => x.Content).Returns(new IItem[] { roomItem1Mock.Object, roomItem2Mock.Object });
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemStaff> staffMock = new Mock<IItemStaff>();
            staffMock.SetupGet(x => x.Name).Returns("staff");
            staffMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            staffMock.SetupGet(x => x.SpellName).Returns("Invisibility");
            userMock.Setup(x => x.GetEquipment<IItemStaff>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => staffMock.Object);

            Staves skill = new Staves(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Staves>(userMock.Object, "brandish");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);
            skill.Execute();

            Assert.IsNull(result);
            auraManagerMock.Verify(x => x.AddAura(inventoryItem1Mock.Object, "Invisibility", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
            auraManagerMock.Verify(x => x.AddAura(inventoryItem2Mock.Object, "Invisibility", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
            auraManagerMock.Verify(x => x.AddAura(userMock.Object, "Invisibility", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
            auraManagerMock.Verify(x => x.AddAura(victimMock.Object, "Invisibility", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
            auraManagerMock.Verify(x => x.AddAura(roomItem1Mock.Object, "Invisibility", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Never);
            auraManagerMock.Verify(x => x.AddAura(roomItem1Mock.Object, "Invisibility", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Never);
        }
    }
}
