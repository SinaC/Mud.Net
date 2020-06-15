using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
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
    public class WandTests : TestBase
    {
        // TODO: wand in inventory and not hold, no charge left, 1 charge left

        // no target + no target spell -> success
        [TestMethod]
        public void NoTarget_NoTargetSpell()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Earthquake", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(typeof(Earthquake)));
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Earthquake(randomManagerMock.Object));

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
            Mock<IItemWand> wandMock = new Mock<IItemWand>();
            wandMock.SetupGet(x => x.Name).Returns("wand");
            wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            wandMock.SetupGet(x => x.SpellName).Returns("Earthquake");
            userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

            Wands skill = new Wands(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Wands>(userMock.Object, "zap");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);
            skill.Execute();

            Assert.IsNull(result);
            userMock.Verify(x => x.Send("The earth trembles beneath your feet!", It.IsAny<object[]>()), Times.Once);
        }

        // no target + character offensive target spell
        [TestMethod]
        public void NoTarget_CharacterOffensiveTargetSpell()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Fireball", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(typeof(Fireball)));
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Fireball(randomManagerMock.Object));

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemWand> wandMock = new Mock<IItemWand>();
            wandMock.SetupGet(x => x.Name).Returns("wand");
            wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            wandMock.SetupGet(x => x.SpellName).Returns("Fireball");
            userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

            Wands skill = new Wands(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Wands>(userMock.Object, "zap");
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
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Armor(randomManagerMock.Object, auraManagerMock.Object));

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
            Mock<IItemWand> wandMock = new Mock<IItemWand>();
            wandMock.SetupGet(x => x.Name).Returns("wand");
            wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            wandMock.SetupGet(x => x.SpellName).Returns("Armor");
            userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

            Wands skill = new Wands(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Wands>(userMock.Object, "zap");
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
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Fireproof(randomManagerMock.Object, auraManagerMock.Object));

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
            Mock<IItemWand> wandMock = new Mock<IItemWand>();
            wandMock.SetupGet(x => x.Name).Returns("wand");
            wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            wandMock.SetupGet(x => x.SpellName).Returns("Fireproof");
            userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

            Wands skill = new Wands(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Wands>(userMock.Object, "zap");
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
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Curse(randomManagerMock.Object, auraManagerMock.Object, dispelManagerMock.Object));

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
            Mock<IItemWand> wandMock = new Mock<IItemWand>();
            wandMock.SetupGet(x => x.Name).Returns("wand");
            wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            wandMock.SetupGet(x => x.SpellName).Returns("Curse");
            userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

            Wands skill = new Wands(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Wands>(userMock.Object, "zap");
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
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Invisibility(randomManagerMock.Object, auraManagerMock.Object));

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
            Mock<IItemWand> wandMock = new Mock<IItemWand>();
            wandMock.SetupGet(x => x.Name).Returns("wand");
            wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            wandMock.SetupGet(x => x.SpellName).Returns("Invisibility");
            userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

            Wands skill = new Wands(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Wands>(userMock.Object, "zap");
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
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Earthquake(randomManagerMock.Object));

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            Mock<ICharacter> targetMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            targetMock.SetupGet(x => x.Name).Returns("target");
            targetMock.SetupGet(x => x.Keywords).Returns("target".Yield());
            targetMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            roomMock.SetupGet(x => x.People).Returns(new ICharacter[] { userMock.Object, targetMock.Object });
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemWand> wandMock = new Mock<IItemWand>();
            wandMock.SetupGet(x => x.Name).Returns("wand");
            wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            wandMock.SetupGet(x => x.SpellName).Returns("Earthquake");
            userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

            Wands skill = new Wands(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Wands>(userMock.Object, "zap target");
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
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Fireball(randomManagerMock.Object));

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            Mock<ICharacter> targetMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            targetMock.SetupGet(x => x.Name).Returns("target");
            targetMock.SetupGet(x => x.Keywords).Returns("target".Yield());
            targetMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            roomMock.SetupGet(x => x.People).Returns(new ICharacter[] { userMock.Object, targetMock.Object });
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemWand> wandMock = new Mock<IItemWand>();
            wandMock.SetupGet(x => x.Name).Returns("wand");
            wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            wandMock.SetupGet(x => x.SpellName).Returns("Fireball");
            userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

            Wands skill = new Wands(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Wands>(userMock.Object, "zap target");
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
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Armor(randomManagerMock.Object, auraManagerMock.Object));

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            Mock<ICharacter> targetMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            targetMock.SetupGet(x => x.Name).Returns("target");
            targetMock.SetupGet(x => x.Keywords).Returns("target".Yield());
            targetMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            roomMock.SetupGet(x => x.People).Returns(new ICharacter[] { userMock.Object, targetMock.Object });
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemWand> wandMock = new Mock<IItemWand>();
            wandMock.SetupGet(x => x.Name).Returns("wand");
            wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            wandMock.SetupGet(x => x.SpellName).Returns("Armor");
            userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

            Wands skill = new Wands(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Wands>(userMock.Object, "zap target");
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
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Fireproof(randomManagerMock.Object, auraManagerMock.Object));

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            Mock<ICharacter> targetMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            targetMock.SetupGet(x => x.Name).Returns("target");
            targetMock.SetupGet(x => x.Keywords).Returns("target".Yield());
            targetMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            roomMock.SetupGet(x => x.People).Returns(new ICharacter[] { userMock.Object, targetMock.Object });
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemWand> wandMock = new Mock<IItemWand>();
            wandMock.SetupGet(x => x.Name).Returns("wand");
            wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            wandMock.SetupGet(x => x.SpellName).Returns("Fireproof");
            userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

            Wands skill = new Wands(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Wands>(userMock.Object, "zap target");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);

            Assert.AreEqual("You can't find it.", result);
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
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Curse(randomManagerMock.Object, auraManagerMock.Object, dispelManagerMock.Object));
 
            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            Mock<ICharacter> targetMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            targetMock.SetupGet(x => x.Name).Returns("target");
            targetMock.SetupGet(x => x.Keywords).Returns("target".Yield());
            targetMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            roomMock.SetupGet(x => x.People).Returns(new ICharacter[] { userMock.Object, targetMock.Object });
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemWand> wandMock = new Mock<IItemWand>();
            wandMock.SetupGet(x => x.Name).Returns("wand");
            wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            wandMock.SetupGet(x => x.SpellName).Returns("Curse");
            userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

            Wands skill = new Wands(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Wands>(userMock.Object, "zap target");
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
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Invisibility(randomManagerMock.Object, auraManagerMock.Object));

            Mock<IArea> areaMock = new Mock<IArea>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            Mock<ICharacter> targetMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            targetMock.SetupGet(x => x.Name).Returns("target");
            targetMock.SetupGet(x => x.Keywords).Returns("target".Yield());
            targetMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            roomMock.SetupGet(x => x.People).Returns(new ICharacter[] { userMock.Object, targetMock.Object });
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemWand> wandMock = new Mock<IItemWand>();
            wandMock.SetupGet(x => x.Name).Returns("wand");
            wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            wandMock.SetupGet(x => x.SpellName).Returns("Invisibility");
            userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

            Wands skill = new Wands(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Wands>(userMock.Object, "zap target");
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
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Earthquake(randomManagerMock.Object));

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
            itemMock.SetupGet(x => x.Keywords).Returns("item".Yield());
            userMock.SetupGet(x => x.Inventory).Returns(itemMock.Object.Yield());
            Mock<IItemWand> wandMock = new Mock<IItemWand>();
            wandMock.SetupGet(x => x.Name).Returns("wand");
            wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            wandMock.SetupGet(x => x.SpellName).Returns("Earthquake");
            userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

            Wands skill = new Wands(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Wands>(userMock.Object, "zap item");
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
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Fireball(randomManagerMock.Object));

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
            itemMock.SetupGet(x => x.Keywords).Returns("item".Yield());
            userMock.SetupGet(x => x.Inventory).Returns(itemMock.Object.Yield());
            Mock<IItemWand> wandMock = new Mock<IItemWand>();
            wandMock.SetupGet(x => x.Name).Returns("wand");
            wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            wandMock.SetupGet(x => x.SpellName).Returns("Fireball");
            userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

            Wands skill = new Wands(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Wands>(userMock.Object, "zap item");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);

            Assert.AreEqual("You can't find it.", result);
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
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Armor(randomManagerMock.Object, auraManagerMock.Object));

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
            itemMock.SetupGet(x => x.Keywords).Returns("item".Yield());
            userMock.SetupGet(x => x.Inventory).Returns(itemMock.Object.Yield());
            Mock<IItemWand> wandMock = new Mock<IItemWand>();
            wandMock.SetupGet(x => x.Name).Returns("wand");
            wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            wandMock.SetupGet(x => x.SpellName).Returns("Armor");
            userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

            Wands skill = new Wands(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Wands>(userMock.Object, "zap item");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);

            Assert.AreEqual("You can't find it.", result);
        }

        // item target + item target spell -> cast on target
        [TestMethod]
        public void ItemTarget_ItemTargetSpell()
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
            itemMock.SetupGet(x => x.Keywords).Returns("item".Yield());
            userMock.SetupGet(x => x.Inventory).Returns(itemMock.Object.Yield());
            Mock<IItemWand> wandMock = new Mock<IItemWand>();
            wandMock.SetupGet(x => x.Name).Returns("wand");
            wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            wandMock.SetupGet(x => x.SpellName).Returns("Fireproof");
            userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

            Wands skill = new Wands(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Wands>(userMock.Object, "zap item");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);
            skill.Execute();

            Assert.IsNull(result);
            userMock.Verify(x => x.Act(ActOptions.ToAll, "{0:N} zap{0:v} {1} with {2}.", It.IsAny<object[]>()), Times.Once);
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
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Curse(randomManagerMock.Object, auraManagerMock.Object, dispelManagerMock.Object));

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
            itemMock.SetupGet(x => x.Keywords).Returns("item".Yield());
            userMock.SetupGet(x => x.Inventory).Returns(itemMock.Object.Yield());
            Mock<IItemWand> wandMock = new Mock<IItemWand>();
            wandMock.SetupGet(x => x.Name).Returns("wand");
            wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            wandMock.SetupGet(x => x.SpellName).Returns("Curse");
            userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

            Wands skill = new Wands(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Wands>(userMock.Object, "zap item");
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
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Invisibility(randomManagerMock.Object, auraManagerMock.Object));

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
            itemMock.SetupGet(x => x.Keywords).Returns("item".Yield());
            userMock.SetupGet(x => x.Inventory).Returns(itemMock.Object.Yield());
            Mock<IItemWand> wandMock = new Mock<IItemWand>();
            wandMock.SetupGet(x => x.Name).Returns("wand");
            wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            wandMock.SetupGet(x => x.SpellName).Returns("Invisibility");
            userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

            Wands skill = new Wands(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Wands>(userMock.Object, "zap item");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);
            skill.Execute();

            Assert.IsNull(result);
            auraManagerMock.Verify(x => x.AddAura(itemMock.Object, "Invisibility", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
        }
    }
}
