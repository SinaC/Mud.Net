using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Common;
using Mud.Container;
using Mud.POC.Abilities2;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Rom24Skills;
using Mud.POC.Abilities2.Rom24Spells;
using Mud.Server.Random;
using System;

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
            DependencyContainer.Current.Register(typeof(Earthquake), () => new Earthquake(randomManagerMock.Object));

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
            scrollMock.SetupGet(x => x.FirstSpell).Returns("Earthquake");
            userMock.SetupGet(x => x.Inventory).Returns(scrollMock.Object.Yield());

            Scrolls skill = new Scrolls(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput(userMock.Object, "recite scroll");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);
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
            DependencyContainer.Current.Register(typeof(Fireball), () => new Fireball(randomManagerMock.Object));

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
            var actionInput = BuildActionInput(userMock.Object, "recite scroll");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);

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
            DependencyContainer.Current.Register(typeof(Armor), () => new Armor(randomManagerMock.Object, auraManagerMock.Object));

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
            var actionInput = BuildActionInput(userMock.Object, "recite scroll");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);
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
            DependencyContainer.Current.Register(typeof(Fireproof), () => new Fireproof(randomManagerMock.Object, auraManagerMock.Object));

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
            var actionInput = BuildActionInput(userMock.Object, "recite scroll");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);

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
            DependencyContainer.Current.Register(typeof(Curse), () => new Curse(randomManagerMock.Object, auraManagerMock.Object, dispelManagerMock.Object));

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
            var actionInput = BuildActionInput(userMock.Object, "recite scroll");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);

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
            DependencyContainer.Current.Register(typeof(Invisibility), () => new Invisibility(randomManagerMock.Object, auraManagerMock.Object));

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
            var actionInput = BuildActionInput(userMock.Object, "recite scroll");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);
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
            DependencyContainer.Current.Register(typeof(Earthquake), () => new Earthquake(randomManagerMock.Object));

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
            var actionInput = BuildActionInput(userMock.Object, "recite scroll target");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);
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
            DependencyContainer.Current.Register(typeof(Fireball), () => new Fireball(randomManagerMock.Object));

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
            var actionInput = BuildActionInput(userMock.Object, "recite scroll target");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);
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
            DependencyContainer.Current.Register(typeof(Armor), () => new Armor(randomManagerMock.Object, auraManagerMock.Object));

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
            var actionInput = BuildActionInput(userMock.Object, "recite scroll target");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);
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
            DependencyContainer.Current.Register(typeof(Fireproof), () => new Fireproof(randomManagerMock.Object, auraManagerMock.Object));

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
            var actionInput = BuildActionInput(userMock.Object, "recite scroll target");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);

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
            DependencyContainer.Current.Register(typeof(Curse), () => new Curse(randomManagerMock.Object, auraManagerMock.Object, dispelManagerMock.Object));

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
            var actionInput = BuildActionInput(userMock.Object, "recite scroll target");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);
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
            DependencyContainer.Current.Register(typeof(Invisibility), () => new Invisibility(randomManagerMock.Object, auraManagerMock.Object));

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
            var actionInput = BuildActionInput(userMock.Object, "recite scroll target");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);
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
            DependencyContainer.Current.Register(typeof(Earthquake), () => new Earthquake(randomManagerMock.Object));

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
            var actionInput = BuildActionInput(userMock.Object, "recite scroll item");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);
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
            DependencyContainer.Current.Register(typeof(Fireball), () => new Fireball(randomManagerMock.Object));

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
            var actionInput = BuildActionInput(userMock.Object, "recite scroll item");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);

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
            DependencyContainer.Current.Register(typeof(Armor), () => new Armor(randomManagerMock.Object, auraManagerMock.Object));

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
            var actionInput = BuildActionInput(userMock.Object, "recite scroll item");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);

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
            DependencyContainer.Current.Register(typeof(Fireproof), () => new Fireproof(randomManagerMock.Object, auraManagerMock.Object));

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
            var actionInput = BuildActionInput(userMock.Object, "recite scroll item");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);
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
            DependencyContainer.Current.Register(typeof(Curse), () => new Curse(randomManagerMock.Object, auraManagerMock.Object, dispelManagerMock.Object));

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
            var actionInput = BuildActionInput(userMock.Object, "recite scroll item");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);
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
            DependencyContainer.Current.Register(typeof(Invisibility), () => new Invisibility(randomManagerMock.Object, auraManagerMock.Object));

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
            var actionInput = BuildActionInput(userMock.Object, "recite scroll item");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);
            skill.Execute();

            Assert.IsNull(result);
            auraManagerMock.Verify(x => x.AddAura(itemMock.Object, "Invisibility", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
        }
    }
}
