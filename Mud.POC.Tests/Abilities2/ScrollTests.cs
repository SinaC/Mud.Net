﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Container;
using Mud.POC.Abilities2;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Rom24Skills;
using Mud.POC.Abilities2.Rom24Spells;
using Mud.Server.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.POC.Tests.Abilities2
{
    [TestClass]
    public class ScrollTests : TestBase
    {
        // TODO: scroll not found in inventory

        // no target + no target spell -> success
        [TestMethod]
        public void NoTarget_NoTargetSpell()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Earthquake", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(typeof(Earthquake)));
            Earthquake spell = new Earthquake(randomManagerMock.Object);
            DependencyContainer.Current.RegisterInstance(spell);

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IItemScroll> scrollMock = new Mock<IItemScroll>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            userMock.SetupGet(x => x.Inventory).Returns(scrollMock.Object.Yield());
            scrollMock.SetupGet(x => x.Name).Returns("scroll");
            scrollMock.SetupGet(x => x.FirstSpell).Returns("Earthquake");
            roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);

            Scrolls skill = new Scrolls(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var parameters = BuildParameters("scroll");
            SkillActionInput actionInput = new SkillActionInput(new AbilityInfo(skill.GetType()), userMock.Object, parameters.rawParameters, parameters.parameters);
            
            string result = skill.Setup(actionInput);
            skill.Execute();

            Assert.IsNull(result);
            userMock.Verify(x => x.Send("The earth trembles beneath your feet!", It.IsAny<object[]>()), Times.Once);
        }

        // no target + character offensive target spell -> fail
        [TestMethod]
        public void NoTarget_CharacterOffensiveTargetSpell()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Fireball", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(typeof(Fireball)));
            Fireball spell = new Fireball(randomManagerMock.Object);
            DependencyContainer.Current.RegisterInstance(spell);

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemScroll> scrollMock = new Mock<IItemScroll>();
            scrollMock.SetupGet(x => x.Name).Returns("scroll");
            scrollMock.SetupGet(x => x.FirstSpell).Returns("Fireball");
            userMock.SetupGet(x => x.Inventory).Returns(scrollMock.Object.Yield());

            Scrolls skill = new Scrolls(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var parameters = BuildParameters("scroll");
            SkillActionInput actionInput = new SkillActionInput(new AbilityInfo(skill.GetType()), userMock.Object, parameters.rawParameters, parameters.parameters);

            string result = skill.Setup(actionInput);

            Assert.AreEqual("Use it on whom?", result);
        }

        // no target + character defensive target spell -> cast on user
        [TestMethod]
        public void NoTarget_CharacterDefensiveTargetSpell()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Armor", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(typeof(Armor)));
            Armor spell = new Armor(randomManagerMock.Object, auraManagerMock.Object);
            DependencyContainer.Current.RegisterInstance(spell);

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemScroll> scrollMock = new Mock<IItemScroll>();
            scrollMock.SetupGet(x => x.Name).Returns("scroll");
            scrollMock.SetupGet(x => x.FirstSpell).Returns("Armor");
            userMock.SetupGet(x => x.Inventory).Returns(scrollMock.Object.Yield());

            Scrolls skill = new Scrolls(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var parameters = BuildParameters("scroll");
            SkillActionInput actionInput = new SkillActionInput(new AbilityInfo(skill.GetType()), userMock.Object, parameters.rawParameters, parameters.parameters);

            string result = skill.Setup(actionInput);
            skill.Execute();

            Assert.IsNull(result);
            userMock.Verify(x => x.Act(ActOptions.ToCharacter, "You feel someone protecting you.", It.IsAny<object[]>()), Times.Once);
        }

        // no target + item target spell -> fail
        [TestMethod]
        public void NoTarget_ItemTargetSpell()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Fireproof", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(typeof(Fireproof)));
            Fireproof spell = new Fireproof(randomManagerMock.Object, auraManagerMock.Object);
            DependencyContainer.Current.RegisterInstance(spell);

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemScroll> scrollMock = new Mock<IItemScroll>();
            scrollMock.SetupGet(x => x.Name).Returns("scroll");
            scrollMock.SetupGet(x => x.FirstSpell).Returns("Fireproof");
            userMock.SetupGet(x => x.Inventory).Returns(scrollMock.Object.Yield());

            Scrolls skill = new Scrolls(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var parameters = BuildParameters("scroll");
            SkillActionInput actionInput = new SkillActionInput(new AbilityInfo(skill.GetType()), userMock.Object, parameters.rawParameters, parameters.parameters);

            string result = skill.Setup(actionInput);

            Assert.AreEqual("What should it be used upon?", result);
        }

        // no target + mixed offensive target spell -> fail
        [TestMethod]
        public void NoTarget_MixedOffensiveTargetSpell()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            Mock<IDispelManager> dispelManagerMock = new Mock<IDispelManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Curse", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(typeof(Curse)));
            Curse spell = new Curse(randomManagerMock.Object, auraManagerMock.Object, dispelManagerMock.Object);
            DependencyContainer.Current.RegisterInstance(spell);

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemScroll> scrollMock = new Mock<IItemScroll>();
            scrollMock.SetupGet(x => x.Name).Returns("scroll");
            scrollMock.SetupGet(x => x.FirstSpell).Returns("Curse");
            userMock.SetupGet(x => x.Inventory).Returns(scrollMock.Object.Yield());

            Scrolls skill = new Scrolls(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var parameters = BuildParameters("scroll");
            SkillActionInput actionInput = new SkillActionInput(new AbilityInfo(skill.GetType()), userMock.Object, parameters.rawParameters, parameters.parameters);

            string result = skill.Setup(actionInput);

            Assert.AreEqual("Use it on whom or what?", result);
        }

        // no target + mixed defensive target spell -> cast on user
        [TestMethod]
        public void NoTarget_MixedDefensiveTargetSpell()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            Mock<IDispelManager> dispelManagerMock = new Mock<IDispelManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Invisibility", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(typeof(Invisibility)));
            Invisibility spell = new Invisibility(randomManagerMock.Object, auraManagerMock.Object);
            DependencyContainer.Current.RegisterInstance(spell);

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemScroll> scrollMock = new Mock<IItemScroll>();
            scrollMock.SetupGet(x => x.Name).Returns("scroll");
            scrollMock.SetupGet(x => x.FirstSpell).Returns("Invisibility");
            userMock.SetupGet(x => x.Inventory).Returns(scrollMock.Object.Yield());

            Scrolls skill = new Scrolls(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var parameters = BuildParameters("scroll");
            SkillActionInput actionInput = new SkillActionInput(new AbilityInfo(skill.GetType()), userMock.Object, parameters.rawParameters, parameters.parameters);

            string result = skill.Setup(actionInput);
            skill.Execute();

            Assert.IsNull(result);
            auraManagerMock.Verify(x => x.AddAura(userMock.Object, "Invisibility", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
        }

        // character target + no target spell -> success
        [TestMethod]
        public void CharacterTarget_NoTargetSpell()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Earthquake", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(typeof(Earthquake)));
            Earthquake spell = new Earthquake(randomManagerMock.Object);
            DependencyContainer.Current.RegisterInstance(spell);

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            Mock<ICharacter> targetMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            targetMock.SetupGet(x => x.Name).Returns("target");
            targetMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            roomMock.SetupGet(x => x.People).Returns(new ICharacter[] { userMock.Object, targetMock.Object });
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemScroll> scrollMock = new Mock<IItemScroll>();
            scrollMock.SetupGet(x => x.Name).Returns("scroll");
            scrollMock.SetupGet(x => x.FirstSpell).Returns("Earthquake");
            userMock.SetupGet(x => x.Inventory).Returns(scrollMock.Object.Yield());

            Scrolls skill = new Scrolls(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var parameters = BuildParameters("scroll target");
            SkillActionInput actionInput = new SkillActionInput(new AbilityInfo(skill.GetType()), userMock.Object, parameters.rawParameters, parameters.parameters);

            string result = skill.Setup(actionInput);
            skill.Execute();

            Assert.IsNull(result);
            userMock.Verify(x => x.Send("The earth trembles beneath your feet!", It.IsAny<object[]>()), Times.Once);
            targetMock.Verify(x => x.AbilityDamage(userMock.Object, It.IsAny<int>(), SchoolTypes.Bash, "earthquake", It.IsAny<bool>()), Times.Once);
        }

        // character target + character offensive target spell -> cast on target
        [TestMethod]
        public void CharacterTarget_CharacterOffensiveSpell()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Fireball", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(typeof(Fireball)));
            Fireball spell = new Fireball(randomManagerMock.Object);
            DependencyContainer.Current.RegisterInstance(spell);

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            Mock<ICharacter> targetMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            targetMock.SetupGet(x => x.Name).Returns("target");
            targetMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            roomMock.SetupGet(x => x.People).Returns(new ICharacter[] { userMock.Object, targetMock.Object });
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemScroll> scrollMock = new Mock<IItemScroll>();
            scrollMock.SetupGet(x => x.Name).Returns("scroll");
            scrollMock.SetupGet(x => x.FirstSpell).Returns("Fireball");
            userMock.SetupGet(x => x.Inventory).Returns(scrollMock.Object.Yield());

            Scrolls skill = new Scrolls(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var parameters = BuildParameters("scroll target");
            SkillActionInput actionInput = new SkillActionInput(new AbilityInfo(skill.GetType()), userMock.Object, parameters.rawParameters, parameters.parameters);

            string result = skill.Setup(actionInput);
            skill.Execute();

            Assert.IsNull(result);
            targetMock.Verify(x => x.AbilityDamage(userMock.Object, It.IsAny<int>(), SchoolTypes.Fire, "fireball", It.IsAny<bool>()), Times.Once);
        }

        // character target + character defensive target spell -> cast on target
        [TestMethod]
        public void CharacterTarget_CharacterDefensiveSpell()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Armor", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(typeof(Armor)));
            Armor spell = new Armor(randomManagerMock.Object, auraManagerMock.Object);
            DependencyContainer.Current.RegisterInstance(spell);

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            Mock<ICharacter> targetMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            targetMock.SetupGet(x => x.Name).Returns("target");
            targetMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            roomMock.SetupGet(x => x.People).Returns(new ICharacter[] { userMock.Object, targetMock.Object });
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemScroll> scrollMock = new Mock<IItemScroll>();
            scrollMock.SetupGet(x => x.Name).Returns("scroll");
            scrollMock.SetupGet(x => x.FirstSpell).Returns("Armor");
            userMock.SetupGet(x => x.Inventory).Returns(scrollMock.Object.Yield());

            Scrolls skill = new Scrolls(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var parameters = BuildParameters("scroll target");
            SkillActionInput actionInput = new SkillActionInput(new AbilityInfo(skill.GetType()), userMock.Object, parameters.rawParameters, parameters.parameters);

            string result = skill.Setup(actionInput);
            skill.Execute();

            Assert.IsNull(result);
            targetMock.Verify(x => x.Act(ActOptions.ToCharacter, "You feel someone protecting you.", It.IsAny<object[]>()), Times.Once);
        }

        // character target + item target spell -> fail
        [TestMethod]
        public void CharacterTarget_ItemTargetSpell()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Fireproof", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(typeof(Fireproof)));
            Fireproof spell = new Fireproof(randomManagerMock.Object, auraManagerMock.Object);
            DependencyContainer.Current.RegisterInstance(spell);

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            Mock<ICharacter> targetMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            targetMock.SetupGet(x => x.Name).Returns("target");
            targetMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            roomMock.SetupGet(x => x.People).Returns(new ICharacter[] { userMock.Object, targetMock.Object });
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemScroll> scrollMock = new Mock<IItemScroll>();
            scrollMock.SetupGet(x => x.Name).Returns("scroll");
            scrollMock.SetupGet(x => x.FirstSpell).Returns("Fireproof");
            userMock.SetupGet(x => x.Inventory).Returns(scrollMock.Object.Yield());

            Scrolls skill = new Scrolls(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var parameters = BuildParameters("scroll target");
            SkillActionInput actionInput = new SkillActionInput(new AbilityInfo(skill.GetType()), userMock.Object, parameters.rawParameters, parameters.parameters);

            string result = skill.Setup(actionInput);

            Assert.AreEqual("You are not carrying that.", result);
        }

        // character target + mixed offensive target spell -> cast on target
        [TestMethod]
        public void CharacterTarget_MixedOffensiveTargetSpell()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            Mock<IDispelManager> dispelManagerMock = new Mock<IDispelManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Curse", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(typeof(Curse)));
            Curse spell = new Curse(randomManagerMock.Object, auraManagerMock.Object, dispelManagerMock.Object);
            DependencyContainer.Current.RegisterInstance(spell);

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            Mock<ICharacter> targetMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            targetMock.SetupGet(x => x.Name).Returns("target");
            targetMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            roomMock.SetupGet(x => x.People).Returns(new ICharacter[] { userMock.Object, targetMock.Object });
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemScroll> scrollMock = new Mock<IItemScroll>();
            scrollMock.SetupGet(x => x.Name).Returns("scroll");
            scrollMock.SetupGet(x => x.FirstSpell).Returns("Curse");
            userMock.SetupGet(x => x.Inventory).Returns(scrollMock.Object.Yield());

            Scrolls skill = new Scrolls(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var parameters = BuildParameters("scroll target");
            SkillActionInput actionInput = new SkillActionInput(new AbilityInfo(skill.GetType()), userMock.Object, parameters.rawParameters, parameters.parameters);

            string result = skill.Setup(actionInput);
            skill.Execute();

            Assert.IsNull(result);
            auraManagerMock.Verify(x => x.AddAura(targetMock.Object, "Curse", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
            targetMock.Verify(x => x.Send("You feel unclean.", It.IsAny<object[]>()), Times.Once);
        }

        // character target + mixed defensive target spell -> cast on target
        [TestMethod]
        public void CharacterTarget_MixedDefensiveTargetSpell()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            Mock<IDispelManager> dispelManagerMock = new Mock<IDispelManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Invisibility", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(typeof(Invisibility)));
            Invisibility spell = new Invisibility(randomManagerMock.Object, auraManagerMock.Object);
            DependencyContainer.Current.RegisterInstance(spell);

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            Mock<ICharacter> targetMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            targetMock.SetupGet(x => x.Name).Returns("target");
            targetMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            roomMock.SetupGet(x => x.People).Returns(new ICharacter[] { userMock.Object, targetMock.Object });
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemScroll> scrollMock = new Mock<IItemScroll>();
            scrollMock.SetupGet(x => x.Name).Returns("scroll");
            scrollMock.SetupGet(x => x.FirstSpell).Returns("Invisibility");
            userMock.SetupGet(x => x.Inventory).Returns(scrollMock.Object.Yield());

            Scrolls skill = new Scrolls(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var parameters = BuildParameters("scroll target");
            SkillActionInput actionInput = new SkillActionInput(new AbilityInfo(skill.GetType()), userMock.Object, parameters.rawParameters, parameters.parameters);

            string result = skill.Setup(actionInput);
            skill.Execute();

            Assert.IsNull(result);
            auraManagerMock.Verify(x => x.AddAura(targetMock.Object, "Invisibility", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
        }

        // item target + no target spell -> success
        [TestMethod]
        public void ItemTarget_NoTargetSpell()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Earthquake", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(typeof(Earthquake)));
            Earthquake spell = new Earthquake(randomManagerMock.Object);
            DependencyContainer.Current.RegisterInstance(spell);

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IItem> itemMock = new Mock<IItem>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            itemMock.SetupGet(x => x.Name).Returns("item");
            Mock<IItemScroll> scrollMock = new Mock<IItemScroll>();
            scrollMock.SetupGet(x => x.Name).Returns("scroll");
            scrollMock.SetupGet(x => x.FirstSpell).Returns("Earthquake");
            userMock.SetupGet(x => x.Inventory).Returns(new IItem[] { scrollMock.Object, itemMock.Object });

            Scrolls skill = new Scrolls(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var parameters = BuildParameters("scroll item");
            SkillActionInput actionInput = new SkillActionInput(new AbilityInfo(skill.GetType()), userMock.Object, parameters.rawParameters, parameters.parameters);

            string result = skill.Setup(actionInput);
            skill.Execute();

            Assert.IsNull(result);
            userMock.Verify(x => x.Send("The earth trembles beneath your feet!", It.IsAny<object[]>()), Times.Once);
        }

        // item target + character offensive target spell -> fail
        [TestMethod]
        public void ItemTarget_CharacterOffensiveTargetSpell()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Fireball", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(typeof(Fireball)));
            Fireball spell = new Fireball(randomManagerMock.Object);
            DependencyContainer.Current.RegisterInstance(spell);

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IItem> itemMock = new Mock<IItem>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            itemMock.SetupGet(x => x.Name).Returns("item");
            Mock<IItemScroll> scrollMock = new Mock<IItemScroll>();
            scrollMock.SetupGet(x => x.Name).Returns("scroll");
            scrollMock.SetupGet(x => x.FirstSpell).Returns("Fireball");
            userMock.SetupGet(x => x.Inventory).Returns(new IItem[] { scrollMock.Object, itemMock.Object });

            Scrolls skill = new Scrolls(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var parameters = BuildParameters("scroll item");
            SkillActionInput actionInput = new SkillActionInput(new AbilityInfo(skill.GetType()), userMock.Object, parameters.rawParameters, parameters.parameters);

            string result = skill.Setup(actionInput);

            Assert.AreEqual("They aren't here.", result);
        }

        // item target + character defensive target spell -> fail
        [TestMethod]
        public void ItemTarget_CharacterDefensiveTargetSpell()

        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Armor", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(typeof(Armor)));
            Armor spell = new Armor(randomManagerMock.Object, auraManagerMock.Object);
            DependencyContainer.Current.RegisterInstance(spell);

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IItem> itemMock = new Mock<IItem>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            itemMock.SetupGet(x => x.Name).Returns("item");
            Mock<IItemScroll> scrollMock = new Mock<IItemScroll>();
            scrollMock.SetupGet(x => x.Name).Returns("scroll");
            scrollMock.SetupGet(x => x.FirstSpell).Returns("Armor");
            userMock.SetupGet(x => x.Inventory).Returns(new IItem[] { scrollMock.Object, itemMock.Object });

            Scrolls skill = new Scrolls(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var parameters = BuildParameters("scroll item");
            SkillActionInput actionInput = new SkillActionInput(new AbilityInfo(skill.GetType()), userMock.Object, parameters.rawParameters, parameters.parameters);

            string result = skill.Setup(actionInput);

            Assert.AreEqual("They aren't here.", result);
        }

        // item target + item target spell -> cast on item
        [TestMethod]
        public void ItemTarget_ItemTargetSpell()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Fireproof", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(typeof(Fireproof)));
            Fireproof spell = new Fireproof(randomManagerMock.Object, auraManagerMock.Object);
            DependencyContainer.Current.RegisterInstance(spell);

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IItem> itemMock = new Mock<IItem>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            itemMock.SetupGet(x => x.Name).Returns("item");
            Mock<IItemScroll> scrollMock = new Mock<IItemScroll>();
            scrollMock.SetupGet(x => x.Name).Returns("scroll");
            scrollMock.SetupGet(x => x.FirstSpell).Returns("Fireproof");
            userMock.SetupGet(x => x.Inventory).Returns(new IItem[] { scrollMock.Object, itemMock.Object });

            Scrolls skill = new Scrolls(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var parameters = BuildParameters("scroll item");
            SkillActionInput actionInput = new SkillActionInput(new AbilityInfo(skill.GetType()), userMock.Object, parameters.rawParameters, parameters.parameters);

            string result = skill.Setup(actionInput);
            skill.Execute();

            Assert.IsNull(result);
            auraManagerMock.Verify(x => x.AddAura(itemMock.Object, "Fireproof", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
            userMock.Verify(x => x.Act(ActOptions.ToCharacter, "You protect {0:N} from fire.", It.IsAny<object[]>()), Times.Once);
        }

        // item target + mixed offensive target spell -> cast on item
        [TestMethod]
        public void ItemTarget_MixedOffensiveTargetSpell()

        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            Mock<IDispelManager> dispelManagerMock = new Mock<IDispelManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Curse", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(typeof(Curse)));
            Curse spell = new Curse(randomManagerMock.Object, auraManagerMock.Object, dispelManagerMock.Object);
            DependencyContainer.Current.RegisterInstance(spell);

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IItem> itemMock = new Mock<IItem>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            itemMock.SetupGet(x => x.Name).Returns("item");
            Mock<IItemScroll> scrollMock = new Mock<IItemScroll>();
            scrollMock.SetupGet(x => x.Name).Returns("scroll");
            scrollMock.SetupGet(x => x.FirstSpell).Returns("Curse");
            userMock.SetupGet(x => x.Inventory).Returns(new IItem[] { scrollMock.Object, itemMock.Object });

            Scrolls skill = new Scrolls(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var parameters = BuildParameters("scroll item");
            SkillActionInput actionInput = new SkillActionInput(new AbilityInfo(skill.GetType()), userMock.Object, parameters.rawParameters, parameters.parameters);

            string result = skill.Setup(actionInput);
            skill.Execute();

            Assert.IsNull(result);
            auraManagerMock.Verify(x => x.AddAura(itemMock.Object, "Curse", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
        }

        // item target + mixed defensive target spell -> cast on item
        [TestMethod]
        public void ItemTarget_MixedDefensiveTargetSpell()

        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            Mock<IDispelManager> dispelManagerMock = new Mock<IDispelManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Invisibility", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(typeof(Invisibility)));
            Invisibility spell = new Invisibility(randomManagerMock.Object, auraManagerMock.Object);
            DependencyContainer.Current.RegisterInstance(spell);

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IItem> itemMock = new Mock<IItem>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            itemMock.SetupGet(x => x.Name).Returns("item");
            Mock<IItemScroll> scrollMock = new Mock<IItemScroll>();
            scrollMock.SetupGet(x => x.Name).Returns("scroll");
            scrollMock.SetupGet(x => x.FirstSpell).Returns("Invisibility");
            userMock.SetupGet(x => x.Inventory).Returns(new IItem[] { scrollMock.Object, itemMock.Object });

            Scrolls skill = new Scrolls(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var parameters = BuildParameters("scroll item");
            SkillActionInput actionInput = new SkillActionInput(new AbilityInfo(skill.GetType()), userMock.Object, parameters.rawParameters, parameters.parameters);

            string result = skill.Setup(actionInput);
            skill.Execute();

            Assert.IsNull(result);
            auraManagerMock.Verify(x => x.AddAura(itemMock.Object, "Invisibility", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
        }
    }
}
