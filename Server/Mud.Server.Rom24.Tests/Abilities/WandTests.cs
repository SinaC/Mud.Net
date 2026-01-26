using Microsoft.Extensions.Logging;
using Moq;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Domain;
using Mud.Flags;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Random;
using Mud.Server.Rom24.Effects;
using Mud.Server.Rom24.Skills;
using Mud.Server.Rom24.Spells;

namespace Mud.Server.Rom24.Tests.Abilities;

[TestClass]
public class WandTests : AbilityTestBase
{
    // TODO: wand in inventory and not hold, no charge left, 1 charge left

    // no target + no target spell -> success
    [TestMethod]
    public void NoTarget_NoTargetSpell()
    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IAbilityManager> abilityManagerMock = new();
        Mock<IItemManager> itemManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

        abilityManagerMock.Setup(x => x.Get("Earthquake", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityDefinition(typeof(Earthquake), []));
        abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Earthquake(new Mock<ILogger<Earthquake>>().Object, randomManagerMock.Object));

        Mock<IArea> areaMock = new();
        Mock<IRoom> roomMock = new();
        Mock<ICharacter> userMock = new();
        userMock.SetupGet(x => x.Name).Returns("user");
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        userMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags());
        userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
        userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
        roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());
        roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
        areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
        Mock<IItemWand> wandMock = new();
        wandMock.SetupGet(x => x.Name).Returns("wand");
        wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
        wandMock.SetupGet(x => x.SpellName).Returns("Earthquake");
        userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

        Wands skill = new(new Mock<ILogger<Wands>>().Object, randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
        var actionInput = BuildActionInput<Wands>(userMock.Object, "zap");
        SkillActionInput skillActionInput = new(actionInput, new AbilityDefinition(skill.GetType(), []), userMock.Object);

        var result = skill.Setup(skillActionInput);
        skill.Execute();

        Assert.IsNull(result);
        userMock.Verify(x => x.Send("The earth trembles beneath your feet!", It.IsAny<object[]>()), Times.Once);
    }

    // no target + character offensive target spell
    [TestMethod]
    public void NoTarget_CharacterOffensiveTargetSpell()
    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IAbilityManager> abilityManagerMock = new();
        Mock<IItemManager> itemManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

        abilityManagerMock.Setup(x => x.Get("Fireball", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityDefinition(typeof(Fireball), []));
        abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Fireball(new Mock<ILogger<Fireball>>().Object, randomManagerMock.Object));

        Mock<IArea> areaMock = new();
        Mock<IRoom> roomMock = new();
        Mock<ICharacter> userMock = new();
        userMock.SetupGet(x => x.Name).Returns("user");
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        userMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags());
        userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
        roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());
        roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
        areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
        Mock<IItemWand> wandMock = new();
        wandMock.SetupGet(x => x.Name).Returns("wand");
        wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
        wandMock.SetupGet(x => x.SpellName).Returns("Fireball");
        userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

        Wands skill = new(new Mock<ILogger<Wands>>().Object, randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
        var actionInput = BuildActionInput<Wands>(userMock.Object, "zap");
        SkillActionInput skillActionInput = new(actionInput, new AbilityDefinition(skill.GetType(), []), userMock.Object);

        var result = skill.Setup(skillActionInput);

        Assert.AreEqual("Use it on whom?", result);
    }

    // no target + character defensive target spell -> cast on user
    [TestMethod]
    public void NoTarget_CharacterDefensiveTargetSpell()
    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IAbilityManager> abilityManagerMock = new();
        Mock<IItemManager> itemManagerMock = new();
        Mock<IAuraManager> auraManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

        abilityManagerMock.Setup(x => x.Get("Armor", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityDefinition(typeof(Armor), []));
        abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Armor(new Mock<ILogger<Armor>>().Object, randomManagerMock.Object, auraManagerMock.Object));

        Mock<IArea> areaMock = new();
        Mock<IRoom> roomMock = new();
        Mock<ICharacter> userMock = new();
        userMock.SetupGet(x => x.Name).Returns("user");
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        userMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags());
        userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
        userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
        roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());
        roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
        areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
        Mock<IItemWand> wandMock = new();
        wandMock.SetupGet(x => x.Name).Returns("wand");
        wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
        wandMock.SetupGet(x => x.SpellName).Returns("Armor");
        userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

        Wands skill = new(new Mock<ILogger<Wands>>().Object, randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
        var actionInput = BuildActionInput<Wands>(userMock.Object, "zap");
        SkillActionInput skillActionInput = new(actionInput, new AbilityDefinition(skill.GetType(), []), userMock.Object);

        var result = skill.Setup(skillActionInput);
        skill.Execute();

        Assert.IsNull(result);
        userMock.Verify(x => x.Act(ActOptions.ToCharacter, "You feel someone protecting you.", It.IsAny<object[]>()), Times.Once);
    }

    // no target + item target spell -> fail
    [TestMethod]
    public void NoTarget_ItemTargetSpell()
    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IAbilityManager> abilityManagerMock = new();
        Mock<IItemManager> itemManagerMock = new();
        Mock<IAuraManager> auraManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

        abilityManagerMock.Setup(x => x.Get("Fireproof", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityDefinition(typeof(Fireproof), []));
        abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Fireproof(new Mock<ILogger<Fireproof>>().Object, randomManagerMock.Object, auraManagerMock.Object));

        Mock<IArea> areaMock = new();
        Mock<IRoom> roomMock = new();
        Mock<ICharacter> userMock = new();
        userMock.SetupGet(x => x.Name).Returns("user");
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        userMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags());
        userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
        userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
        roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());
        roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
        areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
        Mock<IItemWand> wandMock = new();
        wandMock.SetupGet(x => x.Name).Returns("wand");
        wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
        wandMock.SetupGet(x => x.SpellName).Returns("Fireproof");
        userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

        Wands skill = new(new Mock<ILogger<Wands>>().Object, randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
        var actionInput = BuildActionInput<Wands>(userMock.Object, "zap");
        SkillActionInput skillActionInput = new(actionInput, new AbilityDefinition(skill.GetType(), []), userMock.Object);

        var result = skill.Setup(skillActionInput);

        Assert.AreEqual("What should it be used upon?", result);
    }

    // no target + mixed offensive target spell -> fail
    [TestMethod]
    public void NoTarget_MixedOffensiveTargetSpell()
    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IAbilityManager> abilityManagerMock = new();
        Mock<IItemManager> itemManagerMock = new();
        Mock<IAuraManager> auraManagerMock = new();
        Mock<IEffectManager> effectManagerMock = new();
        Mock<IDispelManager> dispelManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

        abilityManagerMock.Setup(x => x.Get("Curse", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityDefinition(typeof(Curse), []));
        abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Curse(new Mock<ILogger<Curse>>().Object, randomManagerMock.Object, auraManagerMock.Object, effectManagerMock.Object, dispelManagerMock.Object));

        Mock<IArea> areaMock = new();
        Mock<IRoom> roomMock = new();
        Mock<ICharacter> userMock = new();
        userMock.SetupGet(x => x.Name).Returns("user");
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        userMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags());
        userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
        userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
        roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());
        roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
        areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
        Mock<IItemWand> wandMock = new();
        wandMock.SetupGet(x => x.Name).Returns("wand");
        wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
        wandMock.SetupGet(x => x.SpellName).Returns("Curse");
        userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);
        effectManagerMock.Setup(x => x.CreateInstance<ICharacter>(It.IsAny<string>()))
            .Returns(() => new CurseEffect(auraManagerMock.Object));

        Wands skill = new(new Mock<ILogger<Wands>>().Object, randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
        var actionInput = BuildActionInput<Wands>(userMock.Object, "zap");
        SkillActionInput skillActionInput = new(actionInput, new AbilityDefinition(skill.GetType(), []), userMock.Object);

        var result = skill.Setup(skillActionInput);

        Assert.AreEqual("Use it on whom or what?", result);
        effectManagerMock.Verify(x => x.CreateInstance<ICharacter>("Curse"), Times.Never);
    }

    // no target + mixed defensive target spell -> cast on user
    [TestMethod]
    public void NoTarget_MixedDefensiveTargetSpell()
    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IAbilityManager> abilityManagerMock = new();
        Mock<IItemManager> itemManagerMock = new();
        Mock<IAuraManager> auraManagerMock = new();
        Mock<IDispelManager> dispelManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

        abilityManagerMock.Setup(x => x.Get("Invisibility", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityDefinition(typeof(Invisibility), []));
        abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Invisibility(new Mock<ILogger<Invisibility>>().Object, randomManagerMock.Object, auraManagerMock.Object));

        Mock<IArea> areaMock = new();
        Mock<IRoom> roomMock = new();
        Mock<ICharacter> userMock = new();
        userMock.SetupGet(x => x.Name).Returns("user");
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        userMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags());
        userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
        userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
        roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());
        roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
        areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
        Mock<IItemWand> wandMock = new();
        wandMock.SetupGet(x => x.Name).Returns("wand");
        wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
        wandMock.SetupGet(x => x.SpellName).Returns("Invisibility");
        userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

        Wands skill = new(new Mock<ILogger<Wands>>().Object, randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
        var actionInput = BuildActionInput<Wands>(userMock.Object, "zap");
        SkillActionInput skillActionInput = new(actionInput, new AbilityDefinition(skill.GetType(), []), userMock.Object);

        var result = skill.Setup(skillActionInput);
        skill.Execute();

        Assert.IsNull(result);
        auraManagerMock.Verify(x => x.AddAura(userMock.Object, "Invisibility", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
    }

    // character target + no target spell -> success
    [TestMethod]
    public void CharacterTarget_NoTargetSpell()
    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IAbilityManager> abilityManagerMock = new();
        Mock<IItemManager> itemManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

        abilityManagerMock.Setup(x => x.Get("Earthquake", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityDefinition(typeof(Earthquake), []));
        abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Earthquake(new Mock<ILogger<Earthquake>>().Object, randomManagerMock.Object));

        Mock<IArea> areaMock = new();
        Mock<IRoom> roomMock = new();
        Mock<ICharacter> userMock = new();
        Mock<ICharacter> targetMock = new();
        userMock.SetupGet(x => x.Name).Returns("user");
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        userMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags());
        userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
        userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
        targetMock.SetupGet(x => x.Name).Returns("target");
        targetMock.SetupGet(x => x.Keywords).Returns("target".Yield());
        targetMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        targetMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags());
        roomMock.SetupGet(x => x.People).Returns([userMock.Object, targetMock.Object]);
        roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
        areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
        Mock<IItemWand> wandMock = new();
        wandMock.SetupGet(x => x.Name).Returns("wand");
        wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
        wandMock.SetupGet(x => x.SpellName).Returns("Earthquake");
        userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

        Wands skill = new(new Mock<ILogger<Wands>>().Object, randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
        var actionInput = BuildActionInput<Wands>(userMock.Object, "zap target");
        SkillActionInput skillActionInput = new(actionInput, new AbilityDefinition(skill.GetType(), []), userMock.Object);

        var result = skill.Setup(skillActionInput);
        skill.Execute();

        Assert.IsNull(result);
        userMock.Verify(x => x.Send("The earth trembles beneath your feet!", It.IsAny<object[]>()), Times.Once);
        targetMock.Verify(x => x.AbilityDamage(userMock.Object, It.IsAny<int>(), SchoolTypes.Bash, "earthquake", It.IsAny<bool>()), Times.Once);
    }

    // character target + character offensive target spell -> cast on target
    [TestMethod]
    public void CharacterTarget_CharacterOffensiveSpell()
    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IAbilityManager> abilityManagerMock = new();
        Mock<IItemManager> itemManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

        abilityManagerMock.Setup(x => x.Get("Fireball", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityDefinition(typeof(Fireball), []));
        abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Fireball(new Mock<ILogger<Fireball>>().Object, randomManagerMock.Object));

        Mock<IArea> areaMock = new();
        Mock<IRoom> roomMock = new();
        Mock<ICharacter> userMock = new();
        Mock<ICharacter> targetMock = new();
        userMock.SetupGet(x => x.Name).Returns("user");
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        userMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags());
        userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
        userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
        targetMock.SetupGet(x => x.Name).Returns("target");
        targetMock.SetupGet(x => x.Keywords).Returns("target".Yield());
        targetMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        targetMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags());
        roomMock.SetupGet(x => x.People).Returns([userMock.Object, targetMock.Object]);
        roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
        areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
        Mock<IItemWand> wandMock = new();
        wandMock.SetupGet(x => x.Name).Returns("wand");
        wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
        wandMock.SetupGet(x => x.SpellName).Returns("Fireball");
        userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

        Wands skill = new(new Mock<ILogger<Wands>>().Object, randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
        var actionInput = BuildActionInput<Wands>(userMock.Object, "zap target");
        SkillActionInput skillActionInput = new(actionInput, new AbilityDefinition(skill.GetType(), []), userMock.Object);

        var result = skill.Setup(skillActionInput);
        skill.Execute();

        Assert.IsNull(result);
        targetMock.Verify(x => x.AbilityDamage(userMock.Object, It.IsAny<int>(), SchoolTypes.Fire, "fireball", It.IsAny<bool>()), Times.Once);
    }

    // character target + character defensive target spell -> cast on target
    [TestMethod]
    public void CharacterTarget_CharacterDefensiveSpell()
    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IAbilityManager> abilityManagerMock = new();
        Mock<IItemManager> itemManagerMock = new();
        Mock<IAuraManager> auraManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

        abilityManagerMock.Setup(x => x.Get("Armor", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityDefinition(typeof(Armor), []));
        abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Armor(new Mock<ILogger<Armor>>().Object, randomManagerMock.Object, auraManagerMock.Object));

        Mock<IArea> areaMock = new();
        Mock<IRoom> roomMock = new();
        Mock<ICharacter> userMock = new();
        Mock<ICharacter> targetMock = new();
        userMock.SetupGet(x => x.Name).Returns("user");
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        userMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags());
        userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
        userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
        targetMock.SetupGet(x => x.Name).Returns("target");
        targetMock.SetupGet(x => x.Keywords).Returns("target".Yield());
        targetMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        targetMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags());
        roomMock.SetupGet(x => x.People).Returns([userMock.Object, targetMock.Object]);
        roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
        areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
        Mock<IItemWand> wandMock = new();
        wandMock.SetupGet(x => x.Name).Returns("wand");
        wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
        wandMock.SetupGet(x => x.SpellName).Returns("Armor");
        userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

        Wands skill = new(new Mock<ILogger<Wands>>().Object, randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
        var actionInput = BuildActionInput<Wands>(userMock.Object, "zap target");
        SkillActionInput skillActionInput = new(actionInput, new AbilityDefinition(skill.GetType(), []), userMock.Object);

        var result = skill.Setup(skillActionInput);
        skill.Execute();

        Assert.IsNull(result);
        targetMock.Verify(x => x.Act(ActOptions.ToCharacter, "You feel someone protecting you.", It.IsAny<object[]>()), Times.Once);
    }

    // character target + item target spell -> fail
    [TestMethod]
    public void CharacterTarget_ItemTargetSpell()
    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IAbilityManager> abilityManagerMock = new();
        Mock<IItemManager> itemManagerMock = new();
        Mock<IAuraManager> auraManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

        abilityManagerMock.Setup(x => x.Get("Fireproof", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityDefinition(typeof(Fireproof), []));
        abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Fireproof(new Mock<ILogger<Fireproof>>().Object, randomManagerMock.Object, auraManagerMock.Object));

        Mock<IArea> areaMock = new();
        Mock<IRoom> roomMock = new();
        Mock<ICharacter> userMock = new();
        Mock<ICharacter> targetMock = new();
        userMock.SetupGet(x => x.Name).Returns("user");
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        userMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags());
        userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
        userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
        targetMock.SetupGet(x => x.Name).Returns("target");
        targetMock.SetupGet(x => x.Keywords).Returns("target".Yield());
        targetMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        targetMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags());
        roomMock.SetupGet(x => x.People).Returns([userMock.Object, targetMock.Object]);
        roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
        areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
        Mock<IItemWand> wandMock = new();
        wandMock.SetupGet(x => x.Name).Returns("wand");
        wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
        wandMock.SetupGet(x => x.SpellName).Returns("Fireproof");
        userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

        Wands skill = new(new Mock<ILogger<Wands>>().Object, randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
        var actionInput = BuildActionInput<Wands>(userMock.Object, "zap target");
        SkillActionInput skillActionInput = new(actionInput, new AbilityDefinition(skill.GetType(), []), userMock.Object);

        var result = skill.Setup(skillActionInput);

        Assert.AreEqual("You can't find it.", result);
    }

    // character target + mixed offensive target spell -> cast on target
    [TestMethod]
    public void CharacterTarget_MixedOffensiveTargetSpell()
    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IAbilityManager> abilityManagerMock = new();
        Mock<IItemManager> itemManagerMock = new();
        Mock<IAuraManager> auraManagerMock = new();
        Mock<IEffectManager> effectManagerMock = new();
        Mock<IDispelManager> dispelManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

        abilityManagerMock.Setup(x => x.Get("Curse", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityDefinition(typeof(Curse), []));
        abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Curse(new Mock<ILogger<Curse>>().Object, randomManagerMock.Object, auraManagerMock.Object, effectManagerMock.Object, dispelManagerMock.Object));

        Mock<IArea> areaMock = new();
        Mock<IRoom> roomMock = new();
        Mock<ICharacter> userMock = new();
        Mock<ICharacter> targetMock = new();
        userMock.SetupGet(x => x.Name).Returns("user");
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        userMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags());
        userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
        userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
        targetMock.SetupGet(x => x.Name).Returns("target");
        targetMock.SetupGet(x => x.Keywords).Returns("target".Yield());
        targetMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        targetMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags());
        roomMock.SetupGet(x => x.People).Returns([userMock.Object, targetMock.Object]);
        roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
        areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
        Mock<IItemWand> wandMock = new();
        wandMock.SetupGet(x => x.Name).Returns("wand");
        wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
        wandMock.SetupGet(x => x.SpellName).Returns("Curse");
        userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);
        effectManagerMock.Setup(x => x.CreateInstance<ICharacter>(It.IsAny<string>()))
            .Returns(() => new CurseEffect(auraManagerMock.Object));

        Wands skill = new(new Mock<ILogger<Wands>>().Object, randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
        var actionInput = BuildActionInput<Wands>(userMock.Object, "zap target");
        SkillActionInput skillActionInput = new(actionInput, new AbilityDefinition(skill.GetType(), []), userMock.Object);

        var result = skill.Setup(skillActionInput);
        skill.Execute();

        Assert.IsNull(result);
        effectManagerMock.Verify(x => x.CreateInstance<ICharacter>("Curse"), Times.Once);
        auraManagerMock.Verify(x => x.AddAura(targetMock.Object, "Curse", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
        targetMock.Verify(x => x.Send("You feel unclean.", It.IsAny<object[]>()), Times.Once);
    }

    // character target + mixed defensive target spell -> cast on target
    [TestMethod]
    public void CharacterTarget_MixedDefensiveTargetSpell()
    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IAbilityManager> abilityManagerMock = new();
        Mock<IItemManager> itemManagerMock = new();
        Mock<IAuraManager> auraManagerMock = new();
        Mock<IDispelManager> dispelManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

        abilityManagerMock.Setup(x => x.Get("Invisibility", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityDefinition(typeof(Invisibility), []));
        abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Invisibility(new Mock<ILogger<Invisibility>>().Object, randomManagerMock.Object, auraManagerMock.Object));

        Mock<IArea> areaMock = new();
        Mock<IRoom> roomMock = new();
        Mock<ICharacter> userMock = new();
        Mock<ICharacter> targetMock = new();
        userMock.SetupGet(x => x.Name).Returns("user");
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        userMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags());
        userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
        userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
        targetMock.SetupGet(x => x.Name).Returns("target");
        targetMock.SetupGet(x => x.Keywords).Returns("target".Yield());
        targetMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        targetMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags());
        roomMock.SetupGet(x => x.People).Returns([userMock.Object, targetMock.Object]);
        roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
        areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
        Mock<IItemWand> wandMock = new();
        wandMock.SetupGet(x => x.Name).Returns("wand");
        wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
        wandMock.SetupGet(x => x.SpellName).Returns("Invisibility");
        userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

        Wands skill = new(new Mock<ILogger<Wands>>().Object, randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
        var actionInput = BuildActionInput<Wands>(userMock.Object, "zap target");
        SkillActionInput skillActionInput = new(actionInput, new AbilityDefinition(skill.GetType(), []), userMock.Object);

        var result = skill.Setup(skillActionInput);
        skill.Execute();

        Assert.IsNull(result);
        auraManagerMock.Verify(x => x.AddAura(targetMock.Object, "Invisibility", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
    }

    // item target + no target spell -> success
    [TestMethod]
    public void ItemTarget_NoTargetSpell()
    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IAbilityManager> abilityManagerMock = new();
        Mock<IItemManager> itemManagerMock = new();
        Mock<IAuraManager> auraManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

        abilityManagerMock.Setup(x => x.Get("Earthquake", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityDefinition(typeof(Earthquake), []));
        abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Earthquake(new Mock<ILogger<Earthquake>>().Object, randomManagerMock.Object));

        Mock<IArea> areaMock = new();
        Mock<IRoom> roomMock = new();
        Mock<IItem> itemMock = new();
        Mock<ICharacter> userMock = new();
        userMock.SetupGet(x => x.Name).Returns("user");
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        userMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags());
        userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
        userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
        roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());
        roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
        areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
        itemMock.SetupGet(x => x.Name).Returns("item");
        itemMock.SetupGet(x => x.Keywords).Returns("item".Yield());
        itemMock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags());
        userMock.SetupGet(x => x.Inventory).Returns(itemMock.Object.Yield());
        Mock<IItemWand> wandMock = new();
        wandMock.SetupGet(x => x.Name).Returns("wand");
        wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
        wandMock.SetupGet(x => x.SpellName).Returns("Earthquake");
        userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

        Wands skill = new(new Mock<ILogger<Wands>>().Object, randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
        var actionInput = BuildActionInput<Wands>(userMock.Object, "zap item");
        SkillActionInput skillActionInput = new(actionInput, new AbilityDefinition(skill.GetType(), []), userMock.Object);

        var result = skill.Setup(skillActionInput);
        skill.Execute();

        Assert.IsNull(result);
        userMock.Verify(x => x.Send("The earth trembles beneath your feet!", It.IsAny<object[]>()), Times.Once);
    }

    // item target + character offensive target spell -> fail
    [TestMethod]
    public void ItemTarget_CharacterOffensiveTargetSpell()
    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IAbilityManager> abilityManagerMock = new();
        Mock<IItemManager> itemManagerMock = new();
        Mock<IAuraManager> auraManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

        abilityManagerMock.Setup(x => x.Get("Fireball", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityDefinition(typeof(Fireball), []));
        abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Fireball(new Mock<ILogger<Fireball>>().Object, randomManagerMock.Object));

        Mock<IArea> areaMock = new();
        Mock<IRoom> roomMock = new();
        Mock<IItem> itemMock = new();
        Mock<ICharacter> userMock = new();
        userMock.SetupGet(x => x.Name).Returns("user");
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        userMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags());
        userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
        userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
        roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());
        roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
        areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
        itemMock.SetupGet(x => x.Name).Returns("item");
        itemMock.SetupGet(x => x.Keywords).Returns("item".Yield());
        itemMock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags());
        userMock.SetupGet(x => x.Inventory).Returns(itemMock.Object.Yield());
        Mock<IItemWand> wandMock = new();
        wandMock.SetupGet(x => x.Name).Returns("wand");
        wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
        wandMock.SetupGet(x => x.SpellName).Returns("Fireball");
        userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

        Wands skill = new(new Mock<ILogger<Wands>>().Object, randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
        var actionInput = BuildActionInput<Wands>(userMock.Object, "zap item");
        SkillActionInput skillActionInput = new(actionInput, new AbilityDefinition(skill.GetType(), []), userMock.Object);

        var result = skill.Setup(skillActionInput);

        Assert.AreEqual("You can't find it.", result);
    }

    // item target + character defensive target spell -> fail
    [TestMethod]
    public void ItemTarget_CharacterDefensiveTargetSpell()

    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IAbilityManager> abilityManagerMock = new();
        Mock<IItemManager> itemManagerMock = new();
        Mock<IAuraManager> auraManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

        abilityManagerMock.Setup(x => x.Get("Armor", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityDefinition(typeof(Armor), []));
        abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Armor(new Mock<ILogger<Armor>>().Object, randomManagerMock.Object, auraManagerMock.Object));

        Mock<IArea> areaMock = new();
        Mock<IRoom> roomMock = new();
        Mock<IItem> itemMock = new();
        Mock<ICharacter> userMock = new();
        userMock.SetupGet(x => x.Name).Returns("user");
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        userMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags());
        userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
        userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
        roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());
        roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
        areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
        itemMock.SetupGet(x => x.Name).Returns("item");
        itemMock.SetupGet(x => x.Keywords).Returns("item".Yield());
        itemMock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags());
        userMock.SetupGet(x => x.Inventory).Returns(itemMock.Object.Yield());
        Mock<IItemWand> wandMock = new();
        wandMock.SetupGet(x => x.Name).Returns("wand");
        wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
        wandMock.SetupGet(x => x.SpellName).Returns("Armor");
        userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

        Wands skill = new(new Mock<ILogger<Wands>>().Object, randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
        var actionInput = BuildActionInput<Wands>(userMock.Object, "zap item");
        SkillActionInput skillActionInput = new(actionInput, new AbilityDefinition(skill.GetType(), []), userMock.Object);

        var result = skill.Setup(skillActionInput);

        Assert.AreEqual("You can't find it.", result);
    }

    // item target + item target spell -> cast on target
    [TestMethod]
    public void ItemTarget_ItemTargetSpell()
    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IAbilityManager> abilityManagerMock = new();
        Mock<IItemManager> itemManagerMock = new();
        Mock<IAuraManager> auraManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

        abilityManagerMock.Setup(x => x.Get("Fireproof", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityDefinition(typeof(Fireproof), []));
        abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Fireproof(new Mock<ILogger<Fireproof>>().Object, randomManagerMock.Object, auraManagerMock.Object));

        Mock<IArea> areaMock = new();
        Mock<IRoom> roomMock = new();
        Mock<IItem> itemMock = new();
        Mock<ICharacter> userMock = new();
        userMock.SetupGet(x => x.Name).Returns("user");
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        userMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags());
        userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
        userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
        roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());
        roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
        areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
        itemMock.SetupGet(x => x.Name).Returns("item");
        itemMock.SetupGet(x => x.Keywords).Returns("item".Yield());
        itemMock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags());
        userMock.SetupGet(x => x.Inventory).Returns(itemMock.Object.Yield());
        Mock<IItemWand> wandMock = new();
        wandMock.SetupGet(x => x.Name).Returns("wand");
        wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
        wandMock.SetupGet(x => x.SpellName).Returns("Fireproof");
        userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

        Wands skill = new(new Mock<ILogger<Wands>>().Object, randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
        var actionInput = BuildActionInput<Wands>(userMock.Object, "zap item");
        SkillActionInput skillActionInput = new(actionInput, new AbilityDefinition(skill.GetType(), []), userMock.Object);

        var result = skill.Setup(skillActionInput);
        skill.Execute();

        Assert.IsNull(result);
        userMock.Verify(x => x.Act(ActOptions.ToAll, "%W%{0:N} zap{0:v} {1} with {2}.%x%", It.IsAny<object[]>()), Times.Once);
        auraManagerMock.Verify(x => x.AddAura(itemMock.Object, "Fireproof", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
        userMock.Verify(x => x.Act(ActOptions.ToCharacter, "You protect {0:N} from fire.", It.IsAny<object[]>()), Times.Once);
    }

    // item target + mixed offensive target spell -> cast on item
    [TestMethod]
    public void ItemTarget_MixedOffensiveTargetSpell()

    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IAbilityManager> abilityManagerMock = new();
        Mock<IItemManager> itemManagerMock = new();
        Mock<IAuraManager> auraManagerMock = new();
        Mock<IEffectManager> effectManagerMock = new();
        Mock<IDispelManager> dispelManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

        abilityManagerMock.Setup(x => x.Get("Curse", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityDefinition(typeof(Curse), []));
        abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Curse(new Mock<ILogger<Curse>>().Object, randomManagerMock.Object, auraManagerMock.Object, effectManagerMock.Object, dispelManagerMock.Object));

        Mock<IArea> areaMock = new();
        Mock<IRoom> roomMock = new();
        Mock<IItem> itemMock = new();
        Mock<ICharacter> userMock = new();
        userMock.SetupGet(x => x.Name).Returns("user");
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        userMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags());
        userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
        userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
        roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());
        roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
        areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
        itemMock.SetupGet(x => x.Name).Returns("item");
        itemMock.SetupGet(x => x.Keywords).Returns("item".Yield());
        itemMock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags());
        userMock.SetupGet(x => x.Inventory).Returns(itemMock.Object.Yield());
        Mock<IItemWand> wandMock = new();
        wandMock.SetupGet(x => x.Name).Returns("wand");
        wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
        wandMock.SetupGet(x => x.SpellName).Returns("Curse");
        userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);
        effectManagerMock.Setup(x => x.CreateInstance<ICharacter>(It.IsAny<string>()))
            .Returns(() => new CurseEffect(auraManagerMock.Object));

        Wands skill = new(new Mock<ILogger<Wands>>().Object, randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
        var actionInput = BuildActionInput<Wands>(userMock.Object, "zap item");
        SkillActionInput skillActionInput = new(actionInput, new AbilityDefinition(skill.GetType(), []), userMock.Object);

        var result = skill.Setup(skillActionInput);
        skill.Execute();

        Assert.IsNull(result);
        // no curse effect on item, only aura
        auraManagerMock.Verify(x => x.AddAura(itemMock.Object, "Curse", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
    }

    // item target + mixed defensive target spell -> cast on item
    [TestMethod]
    public void ItemTarget_MixedDefensiveTargetSpell()

    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IAbilityManager> abilityManagerMock = new();
        Mock<IItemManager> itemManagerMock = new();
        Mock<IAuraManager> auraManagerMock = new();
        Mock<IDispelManager> dispelManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);

        abilityManagerMock.Setup(x => x.Get("Invisibility", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityDefinition(typeof(Invisibility), []));
        abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Invisibility(new Mock<ILogger<Invisibility>>().Object, randomManagerMock.Object, auraManagerMock.Object));

        Mock<IArea> areaMock = new();
        Mock<IRoom> roomMock = new();
        Mock<IItem> itemMock = new();
        Mock<ICharacter> userMock = new();
        userMock.SetupGet(x => x.Name).Returns("user");
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        userMock.SetupGet(x => x.CharacterFlags).Returns(() => new CharacterFlags());
        userMock.Setup(x => x.CanSee(It.IsAny<IItem>())).Returns<IItem>(_ => true);
        userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => true);
        roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());
        roomMock.SetupGet(x => x.Area).Returns(areaMock.Object);
        areaMock.SetupGet(x => x.Characters).Returns(roomMock.Object.People);
        itemMock.SetupGet(x => x.Name).Returns("item");
        itemMock.SetupGet(x => x.Keywords).Returns("item".Yield());
        itemMock.SetupGet(x => x.ItemFlags).Returns(() => new ItemFlags());
        userMock.SetupGet(x => x.Inventory).Returns(itemMock.Object.Yield());
        Mock<IItemWand> wandMock = new();
        wandMock.SetupGet(x => x.Name).Returns("wand");
        wandMock.SetupGet(x => x.CurrentChargeCount).Returns(2);
        wandMock.SetupGet(x => x.SpellName).Returns("Invisibility");
        userMock.Setup(x => x.GetEquipment<IItemWand>(EquipmentSlots.OffHand)).Returns<EquipmentSlots>(_ => wandMock.Object);

        Wands skill = new(new Mock<ILogger<Wands>>().Object, randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object);
        var actionInput = BuildActionInput<Wands>(userMock.Object, "zap item");
        SkillActionInput skillActionInput = new(actionInput, new AbilityDefinition(skill.GetType(), []), userMock.Object);

        var result = skill.Setup(skillActionInput);
        skill.Execute();

        Assert.IsNull(result);
        auraManagerMock.Verify(x => x.AddAura(itemMock.Object, "Invisibility", userMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
    }
}
