using Microsoft.Extensions.Logging;
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
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;
using Mud.Server.Rom24.Flags;
using Mud.Server.Rom24.Skills;
using Mud.Server.Rom24.Spells;

namespace Mud.Server.Rom24.Tests.Abilities
{
    [TestClass]
    public class StavesTests : AbilityTestBase
    {
        // set 2 characters in room, 2 items in inventory, 2 items in room

        // no target spell -> invalid spell on staves
        [TestMethod]
        public void NoTargetSpell()
        {
            Mock<IRandomManager> randomManagerMock = new();
            Mock<IAbilityManager> abilityManagerMock = new();
            Mock<IItemManager> itemManagerMock = new();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Earthquake", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(new Mock<ILogger>().Object, typeof(Earthquake)));
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Earthquake(new Mock<ILogger<Earthquake>>().Object, randomManagerMock.Object));

            Mock<IArea> areaMock = new();
            Mock<IRoom> roomMock = new();
            Mock<IItem> inventoryItem1Mock = new();
            Mock<IItem> inventoryItem2Mock = new();
            Mock<IItem> roomItem1Mock = new();
            Mock<IItem> roomItem2Mock = new();
            Mock<ICharacter> userMock = new();
            Mock<ICharacter> victimMock = new();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags(new CharacterFlagValues(new Mock<ILogger<CharacterFlagValues>>().Object)));
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            victimMock.SetupGet(x => x.Name).Returns("victim");
            victimMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            victimMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            victimMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            victimMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags(new CharacterFlagValues(new Mock<ILogger<CharacterFlagValues>>().Object)));
            victimMock.Setup(x => x.Inventory).Returns([inventoryItem1Mock.Object, inventoryItem2Mock.Object]);
            inventoryItem1Mock.SetupGet(x => x.Name).Returns("inventoryitem1");
            inventoryItem1Mock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags(new ItemFlagValues(new Mock<ILogger<ItemFlagValues>>().Object)));
            inventoryItem2Mock.SetupGet(x => x.Name).Returns("inventoryitem2");
            inventoryItem2Mock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags(new ItemFlagValues(new Mock<ILogger<ItemFlagValues>>().Object)));
            roomItem1Mock.SetupGet(x => x.Name).Returns("roomItem1");
            roomItem1Mock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags(new ItemFlagValues(new Mock<ILogger<ItemFlagValues>>().Object)));
            roomItem2Mock.SetupGet(x => x.Name).Returns("roomItem2");
            roomItem2Mock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags(new ItemFlagValues(new Mock<ILogger<ItemFlagValues>>().Object)));
            roomMock.SetupGet(x => x.People).Returns( [userMock.Object, victimMock.Object]);
            roomMock.SetupGet(x => x.Content).Returns([roomItem1Mock.Object, roomItem2Mock.Object]);
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemStaff> staffMock = new();
            staffMock.SetupGet(x => x.Name).Returns("staff");
            staffMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            staffMock.SetupGet(x => x.SpellName).Returns("Earthquake");
            userMock.Setup(x => x.GetEquipment<IItemStaff>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => staffMock.Object);

            Staves skill = new(new Mock<ILogger<Staves>>().Object, randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Staves>(userMock.Object, "brandish");
            SkillActionInput skillActionInput = new(actionInput, new AbilityInfo(new Mock<ILogger>().Object, skill.GetType()), userMock.Object);

            var result = skill.Setup(skillActionInput);

            Assert.AreEqual("Something goes wrong.", result);
        }

        // character offensive target spell -> cast on each target
        [TestMethod]
        public void CharacterOffensiveTargetSpell()
        {
            Mock<IRandomManager> randomManagerMock = new();
            Mock<IAbilityManager> abilityManagerMock = new();
            Mock<IItemManager> itemManagerMock = new();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Fireball", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(new Mock<ILogger>().Object, typeof(Fireball)));
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Fireball(new Mock<ILogger<Fireball>>().Object, randomManagerMock.Object));

            Mock<IArea> areaMock = new();
            Mock<IRoom> roomMock = new();
            Mock<IItem> inventoryItem1Mock = new();
            Mock<IItem> inventoryItem2Mock = new();
            Mock<IItem> roomItem1Mock = new();
            Mock<IItem> roomItem2Mock = new();
            Mock<ICharacter> userMock = new();
            Mock<ICharacter> victimMock = new();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags(new CharacterFlagValues(new Mock<ILogger<CharacterFlagValues>>().Object)));
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            userMock.Setup(x => x.Inventory).Returns([inventoryItem1Mock.Object, inventoryItem2Mock.Object]);
            victimMock.SetupGet(x => x.Name).Returns("victim");
            victimMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            victimMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            victimMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            victimMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags(new CharacterFlagValues(new Mock<ILogger<CharacterFlagValues>>().Object)));
            inventoryItem1Mock.SetupGet(x => x.Name).Returns("inventoryitem1");
            inventoryItem1Mock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags(new ItemFlagValues(new Mock<ILogger<ItemFlagValues>>().Object)));
            inventoryItem2Mock.SetupGet(x => x.Name).Returns("inventoryitem2");
            inventoryItem2Mock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags(new ItemFlagValues(new Mock<ILogger<ItemFlagValues>>().Object)));
            roomItem1Mock.SetupGet(x => x.Name).Returns("roomItem1");
            roomItem1Mock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags(new ItemFlagValues(new Mock<ILogger<ItemFlagValues>>().Object)));
            roomItem2Mock.SetupGet(x => x.Name).Returns("roomItem2");
            roomItem2Mock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags(new ItemFlagValues(new Mock<ILogger<ItemFlagValues>>().Object)));
            roomMock.SetupGet(x => x.People).Returns([userMock.Object, victimMock.Object]);
            roomMock.SetupGet(x => x.Content).Returns([roomItem1Mock.Object, roomItem2Mock.Object]);
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemStaff> staffMock = new();
            staffMock.SetupGet(x => x.Name).Returns("staff");
            staffMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            staffMock.SetupGet(x => x.SpellName).Returns("Fireball");
            userMock.Setup(x => x.GetEquipment<IItemStaff>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => staffMock.Object);

            Staves skill = new(new Mock<ILogger<Staves>>().Object, randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Staves>(userMock.Object, "brandish");
            SkillActionInput skillActionInput = new(actionInput, new AbilityInfo(new Mock<ILogger>().Object, skill.GetType()), userMock.Object);

            var result = skill.Setup(skillActionInput);
            skill.Execute();

            Assert.IsNull(result);
            userMock.Verify(x => x.AbilityDamage(userMock.Object, It.IsAny<int>(), SchoolTypes.Fire, "fireball", It.IsAny<bool>()), Times.Never);
            victimMock.Verify(x => x.AbilityDamage(userMock.Object, It.IsAny<int>(), SchoolTypes.Fire, "fireball", It.IsAny<bool>()), Times.Once);
        }

        // character offensive target spell -> cast on each target
        [TestMethod]
        public void CharacterDefensiveTargetSpell()
        {
            Mock<IRandomManager> randomManagerMock = new();
            Mock<IAbilityManager> abilityManagerMock = new();
            Mock<IItemManager> itemManagerMock = new();
            Mock<IAuraManager> auraManagerMock = new();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Armor", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(new Mock<ILogger>().Object, typeof(Armor)));
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Armor(new Mock<ILogger<Armor>>().Object, randomManagerMock.Object, auraManagerMock.Object));

            Mock<IArea> areaMock = new();
            Mock<IRoom> roomMock = new();
            Mock<IItem> inventoryItem1Mock = new();
            Mock<IItem> inventoryItem2Mock = new();
            Mock<IItem> roomItem1Mock = new();
            Mock<IItem> roomItem2Mock = new();
            Mock<ICharacter> userMock = new();
            Mock<ICharacter> victimMock = new();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags(new CharacterFlagValues(new Mock<ILogger<CharacterFlagValues>>().Object)));
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            userMock.Setup(x => x.Inventory).Returns([inventoryItem1Mock.Object, inventoryItem2Mock.Object]);
            victimMock.SetupGet(x => x.Name).Returns("victim");
            victimMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            victimMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            victimMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            victimMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags(new CharacterFlagValues(new Mock<ILogger<CharacterFlagValues>>().Object)));
            inventoryItem1Mock.SetupGet(x => x.Name).Returns("inventoryitem1");
            inventoryItem1Mock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags(new ItemFlagValues(new Mock<ILogger<ItemFlagValues>>().Object)));
            inventoryItem2Mock.SetupGet(x => x.Name).Returns("inventoryitem2");
            inventoryItem2Mock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags(new ItemFlagValues(new Mock<ILogger<ItemFlagValues>>().Object)));
            roomItem1Mock.SetupGet(x => x.Name).Returns("roomItem1");
            roomItem1Mock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags(new ItemFlagValues(new Mock<ILogger<ItemFlagValues>>().Object)));
            roomItem2Mock.SetupGet(x => x.Name).Returns("roomItem2");
            roomItem2Mock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags(new ItemFlagValues(new Mock<ILogger<ItemFlagValues>>().Object)));
            roomMock.SetupGet(x => x.People).Returns([userMock.Object, victimMock.Object]);
            roomMock.SetupGet(x => x.Content).Returns([roomItem1Mock.Object, roomItem2Mock.Object]);
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemStaff> staffMock = new();
            staffMock.SetupGet(x => x.Name).Returns("staff");
            staffMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            staffMock.SetupGet(x => x.SpellName).Returns("Armor");
            userMock.Setup(x => x.GetEquipment<IItemStaff>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => staffMock.Object);

            Staves skill = new(new Mock<ILogger<Staves>>().Object, randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Staves>(userMock.Object, "brandish");
            SkillActionInput skillActionInput = new(actionInput, new AbilityInfo(new Mock<ILogger>().Object, skill.GetType()), userMock.Object);

            var result = skill.Setup(skillActionInput);
            skill.Execute();

            Assert.IsNull(result);
            userMock.Verify(x => x.Act(ActOptions.ToCharacter, "You feel someone protecting you.", It.IsAny<object[]>()), Times.Once);
            victimMock.Verify(x => x.Act(ActOptions.ToCharacter, "You feel someone protecting you.", It.IsAny<object[]>()), Times.Once);
        }

        // item target spell -> cast on each item in inventory
        [TestMethod]
        public void ItemTargetSpell()
        {
            Mock<IRandomManager> randomManagerMock = new();
            Mock<IAbilityManager> abilityManagerMock = new();
            Mock<IItemManager> itemManagerMock = new();
            Mock<IAuraManager> auraManagerMock = new();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Fireproof", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(new Mock<ILogger>().Object, typeof(Fireproof)));
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Fireproof(new Mock<ILogger<Fireproof>>().Object, _itemFlagFactory, randomManagerMock.Object, auraManagerMock.Object));

            Mock<IArea> areaMock = new();
            Mock<IRoom> roomMock = new();
            Mock<IItem> inventoryItem1Mock = new();
            Mock<IItem> inventoryItem2Mock = new();
            Mock<IItem> roomItem1Mock = new();
            Mock<IItem> roomItem2Mock = new();
            Mock<ICharacter> userMock = new();
            Mock<ICharacter> victimMock = new();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags(new CharacterFlagValues(new Mock<ILogger<CharacterFlagValues>>().Object)));
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            userMock.Setup(x => x.Inventory).Returns([inventoryItem1Mock.Object, inventoryItem2Mock.Object]);
            victimMock.SetupGet(x => x.Name).Returns("victim");
            victimMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            victimMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            victimMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            victimMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags(new CharacterFlagValues(new Mock<ILogger<CharacterFlagValues>>().Object)));
            inventoryItem1Mock.SetupGet(x => x.Name).Returns("inventoryitem1");
            inventoryItem1Mock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags(new ItemFlagValues(new Mock<ILogger<ItemFlagValues>>().Object)));
            inventoryItem2Mock.SetupGet(x => x.Name).Returns("inventoryitem2");
            inventoryItem2Mock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags(new ItemFlagValues(new Mock<ILogger<ItemFlagValues>>().Object)));
            roomItem1Mock.SetupGet(x => x.Name).Returns("roomItem1");
            roomItem1Mock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags(new ItemFlagValues(new Mock<ILogger<ItemFlagValues>>().Object)));
            roomItem2Mock.SetupGet(x => x.Name).Returns("roomItem2");
            roomItem2Mock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags(new ItemFlagValues(new Mock<ILogger<ItemFlagValues>>().Object)));
            roomMock.SetupGet(x => x.People).Returns([userMock.Object, victimMock.Object]);
            roomMock.SetupGet(x => x.Content).Returns([roomItem1Mock.Object, roomItem2Mock.Object]);
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemStaff> staffMock = new();
            staffMock.SetupGet(x => x.Name).Returns("staff");
            staffMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            staffMock.SetupGet(x => x.SpellName).Returns("Fireproof");
            userMock.Setup(x => x.GetEquipment<IItemStaff>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => staffMock.Object);

            Staves skill = new(new Mock<ILogger<Staves>>().Object, randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Staves>(userMock.Object, "brandish");
            SkillActionInput skillActionInput = new(actionInput, new AbilityInfo(new Mock<ILogger>().Object, skill.GetType()), userMock.Object);

            var result = skill.Setup(skillActionInput);
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
            Mock<IRandomManager> randomManagerMock = new();
            Mock<IAbilityManager> abilityManagerMock = new();
            Mock<IItemManager> itemManagerMock = new();
            Mock<IAuraManager> auraManagerMock = new();
            Mock<IEffectManager> effectManagerMock = new();
            Mock<IDispelManager> dispelManagerMock = new();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Curse", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(new Mock<ILogger>().Object, typeof(Curse)));
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Curse(new Mock<ILogger<Curse>>().Object, _itemFlagFactory, randomManagerMock.Object, auraManagerMock.Object, effectManagerMock.Object, dispelManagerMock.Object));

            Mock<IArea> areaMock = new();
            Mock<IRoom> roomMock = new();
            Mock<IItem> inventoryItem1Mock = new();
            Mock<IItem> inventoryItem2Mock = new();
            Mock<IItem> roomItem1Mock = new();
            Mock<IItem> roomItem2Mock = new();
            Mock<ICharacter> userMock = new();
            Mock<ICharacter> victim1Mock = new();
            Mock<ICharacter> victim2Mock = new();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags(new CharacterFlagValues(new Mock<ILogger<CharacterFlagValues>>().Object)));
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            userMock.Setup(x => x.Inventory).Returns([inventoryItem1Mock.Object, inventoryItem2Mock.Object]);
            victim1Mock.SetupGet(x => x.Name).Returns("victim");
            victim1Mock.SetupGet(x => x.Room).Returns(roomMock.Object);
            victim1Mock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags(new CharacterFlagValues(new Mock<ILogger<CharacterFlagValues>>().Object)));
            victim1Mock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            victim1Mock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            victim2Mock.SetupGet(x => x.Name).Returns("victim");
            victim2Mock.SetupGet(x => x.Room).Returns(roomMock.Object);
            victim2Mock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags(new CharacterFlagValues(new Mock<ILogger<CharacterFlagValues>>().Object)));
            victim2Mock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            victim2Mock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            inventoryItem1Mock.SetupGet(x => x.Name).Returns("inventoryitem1");
            inventoryItem1Mock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags(new ItemFlagValues(new Mock<ILogger<ItemFlagValues>>().Object)));
            inventoryItem2Mock.SetupGet(x => x.Name).Returns("inventoryitem2");
            inventoryItem2Mock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags(new ItemFlagValues(new Mock<ILogger<ItemFlagValues>>().Object)));
            roomItem1Mock.SetupGet(x => x.Name).Returns("roomItem1");
            roomItem1Mock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags(new ItemFlagValues(new Mock<ILogger<ItemFlagValues>>().Object)));
            roomItem2Mock.SetupGet(x => x.Name).Returns("roomItem2");
            roomItem2Mock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags(new ItemFlagValues(new Mock<ILogger<ItemFlagValues>>().Object)));
            roomMock.SetupGet(x => x.People).Returns([userMock.Object, victim1Mock.Object, victim2Mock.Object]);
            roomMock.SetupGet(x => x.Content).Returns([roomItem1Mock.Object, roomItem2Mock.Object]);
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemStaff> staffMock = new();
            staffMock.SetupGet(x => x.Name).Returns("staff");
            staffMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            staffMock.SetupGet(x => x.SpellName).Returns("Curse");
            userMock.Setup(x => x.GetEquipment<IItemStaff>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => staffMock.Object);
            effectManagerMock.Setup(x => x.CreateInstance<ICharacter>(It.IsAny<string>()))
                .Returns(() => new CurseEffect(_characterFlagFactory, auraManagerMock.Object));

            Staves skill = new(new Mock<ILogger<Staves>>().Object, randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Staves>(userMock.Object, "brandish");
            SkillActionInput skillActionInput = new(actionInput, new AbilityInfo(new Mock<ILogger>().Object, skill.GetType()), userMock.Object);

            var result = skill.Setup(skillActionInput);
            skill.Execute();

            Assert.IsNull(result);
            effectManagerMock.Verify(x => x.CreateInstance<ICharacter>("Curse"), Times.Exactly(2)); // 2 victims will generate an effect
            auraManagerMock.Verify(x => x.AddAura(inventoryItem1Mock.Object, "Curse", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
            auraManagerMock.Verify(x => x.AddAura(inventoryItem2Mock.Object, "Curse", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
            auraManagerMock.Verify(x => x.AddAura(userMock.Object, "Curse", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Never);
            auraManagerMock.Verify(x => x.AddAura(victim1Mock.Object, "Curse", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
            auraManagerMock.Verify(x => x.AddAura(victim2Mock.Object, "Curse", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
            auraManagerMock.Verify(x => x.AddAura(roomItem1Mock.Object, "Curse", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
            auraManagerMock.Verify(x => x.AddAura(roomItem1Mock.Object, "Curse", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
        }

        // mixed defensive target spell -> user, victim, items in inventory
        [TestMethod]
        public void MixedDefensiveTargetSpell()
        {
            Mock<IRandomManager> randomManagerMock = new();
            Mock<IAbilityManager> abilityManagerMock = new();
            Mock<IItemManager> itemManagerMock = new();
            Mock<IAuraManager> auraManagerMock = new();
            Mock<IDispelManager> dispelManagerMock = new();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

            abilityManagerMock.Setup(x => x.Search("Invisibility", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(new Mock<ILogger>().Object, typeof(Invisibility)));
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Invisibility(new Mock<ILogger<Invisibility>>().Object, _characterFlagFactory, _itemFlagFactory, randomManagerMock.Object, auraManagerMock.Object));

            Mock<IArea> areaMock = new();
            Mock<IRoom> roomMock = new();
            Mock<IItem> inventoryItem1Mock = new();
            Mock<IItem> inventoryItem2Mock = new();
            Mock<IItem> roomItem1Mock = new();
            Mock<IItem> roomItem2Mock = new();
            Mock<ICharacter> userMock = new();
            Mock<ICharacter> victimMock = new();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            userMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags(new CharacterFlagValues(new Mock<ILogger<CharacterFlagValues>>().Object)));
            userMock.Setup(x => x.Inventory).Returns([inventoryItem1Mock.Object, inventoryItem2Mock.Object]);
            victimMock.SetupGet(x => x.Name).Returns("victim");
            victimMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            victimMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags(new CharacterFlagValues(new Mock<ILogger<CharacterFlagValues>>().Object)));
            victimMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
            victimMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
            inventoryItem1Mock.SetupGet(x => x.Name).Returns("inventoryitem1");
            inventoryItem1Mock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags(new ItemFlagValues(new Mock<ILogger<ItemFlagValues>>().Object)));
            inventoryItem2Mock.SetupGet(x => x.Name).Returns("inventoryitem2");
            inventoryItem2Mock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags(new ItemFlagValues(new Mock<ILogger<ItemFlagValues>>().Object)));
            roomItem1Mock.SetupGet(x => x.Name).Returns("roomItem1");
            roomItem1Mock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags(new ItemFlagValues(new Mock<ILogger<ItemFlagValues>>().Object)));
            roomItem2Mock.SetupGet(x => x.Name).Returns("roomItem2");
            roomItem2Mock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags(new ItemFlagValues(new Mock<ILogger<ItemFlagValues>>().Object)));
            roomMock.SetupGet(x => x.People).Returns([userMock.Object, victimMock.Object]);
            roomMock.SetupGet(x => x.Content).Returns([roomItem1Mock.Object, roomItem2Mock.Object]);
            roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
            areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
            Mock<IItemStaff> staffMock = new();
            staffMock.SetupGet(x => x.Name).Returns("staff");
            staffMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
            staffMock.SetupGet(x => x.SpellName).Returns("Invisibility");
            userMock.Setup(x => x.GetEquipment<IItemStaff>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => staffMock.Object);

            Staves skill = new(new Mock<ILogger<Staves>>().Object, randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
            var actionInput = BuildActionInput<Staves>(userMock.Object, "brandish");
            SkillActionInput skillActionInput = new(actionInput, new AbilityInfo(new Mock<ILogger>().Object, skill.GetType()), userMock.Object);

            var result = skill.Setup(skillActionInput);
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
