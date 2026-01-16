using Microsoft.Extensions.Logging;
using Moq;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Flags;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Room;
using Mud.Random;
using Mud.Server.Rom24.Skills;

namespace Mud.Server.Rom24.Tests.Abilities;

[TestClass]
public class BerserkTests : AbilityTestBase
{
    [TestMethod]
    public void PcNotKnown()
    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IAuraManager> auraManagerMock = new();

        Mock<IPlayableCharacter> userMock = new();
        Mock<IRoom> roomMock = new();
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());

        var skill = new Berserk(new Mock<ILogger<Berserk>>().Object, randomManagerMock.Object, auraManagerMock.Object);
        var actionInput = BuildActionInput<Berserk>(userMock.Object, "berserk");
        var skillActionInput = new SkillActionInput(actionInput, new AbilityDefinition(skill.GetType(), []), userMock.Object);

        var result = skill.Setup(skillActionInput);

        Assert.AreEqual("You turn red in the face, but nothing happens.", result);
    }

    [TestMethod]
    public void NpcNotKnownNoOffensiveFlags()
    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IAuraManager> auraManagerMock = new();

        Mock<INonPlayableCharacter> userMock = new();
        Mock<IRoom> roomMock = new();
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());

        var skill = new Berserk(new Mock<ILogger<Berserk>>().Object, randomManagerMock.Object, auraManagerMock.Object);
        var actionInput = BuildActionInput<Berserk>(userMock.Object, "berserk");
        var skillActionInput = new SkillActionInput(actionInput, new AbilityDefinition(skill.GetType(), []), userMock.Object);

        var result = skill.Setup(skillActionInput);

        Assert.AreEqual("You turn red in the face, but nothing happens.", result);
    }

    [TestMethod]
    public void NpcNotKnownBerserkOffensiveFlags()
    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IAuraManager> auraManagerMock = new();

        Mock<INonPlayableCharacter> userMock = new();
        Mock<IRoom> roomMock = new();
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        userMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
        userMock.SetupGet(x => x.OffensiveFlags).Returns(new OffensiveFlags("Berserk"));
        userMock.SetupGet(x => x.CurrentResourceKinds).Returns([ResourceKinds.Mana]); // has mana
        userMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(1000); // enough mana
        roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());

        var skill = new Berserk(new Mock<ILogger<Berserk>>().Object, randomManagerMock.Object, auraManagerMock.Object);
        var actionInput = BuildActionInput<Berserk>(userMock.Object, "berserk");
        var skillActionInput = new SkillActionInput(actionInput, new AbilityDefinition(skill.GetType(), []), userMock.Object);

        var result = skill.Setup(skillActionInput);

        Assert.AreEqual("You turn red in the face, but nothing happens.", result);
    }

    [TestMethod]
    public void HasBerserkFlags()
    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IAuraManager> auraManagerMock = new();

        Mock<IPlayableCharacter> userMock = new();
        Mock<IRoom> roomMock = new();
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        userMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
        userMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
        userMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags("Berserk"));
        userMock.SetupGet(x => x.CurrentResourceKinds).Returns([ResourceKinds.Mana]); // has mana
        userMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(1000); // enough mana
        roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());

        var skill = new Berserk(new Mock<ILogger<Berserk>>().Object, randomManagerMock.Object, auraManagerMock.Object);
        var actionInput = BuildActionInput<Berserk>(userMock.Object, "berserk");
        var skillActionInput = new SkillActionInput(actionInput, new AbilityDefinition(skill.GetType(), []), userMock.Object);

        var result = skill.Setup(skillActionInput);

        Assert.AreEqual("You get a little madder.", result);
    }

    [TestMethod]
    public void AffectedByBerserk()
    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IAuraManager> auraManagerMock = new();

        Mock<IPlayableCharacter> userMock = new();
        Mock<IRoom> roomMock = new();
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        userMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
        userMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
        userMock.Setup(x => x.GetAura("Berserk")).Returns<string>(_ => new Mock<IAura>().Object);
        userMock.SetupGet(x => x.CurrentResourceKinds).Returns([ResourceKinds.Mana]); // has mana
        userMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(1000); // enough mana
        roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());

        var skill = new Berserk(new Mock<ILogger<Berserk>>().Object, randomManagerMock.Object, auraManagerMock.Object);
        var actionInput = BuildActionInput<Berserk>(userMock.Object, "berserk");
        var skillActionInput = new SkillActionInput(actionInput, new AbilityDefinition(skill.GetType(), []), userMock.Object);

        var result = skill.Setup(skillActionInput);

        Assert.AreEqual("You get a little madder.", result);
    }

    [TestMethod]
    public void HasCalmFlag()
    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IAuraManager> auraManagerMock = new();

        Mock<IPlayableCharacter> userMock = new();
        Mock<IRoom> roomMock = new();
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        userMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
        userMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
        userMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags("Calm"));
        userMock.SetupGet(x => x.CurrentResourceKinds).Returns([ResourceKinds.Mana]); // has mana
        userMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(1000); // enough mana
        roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());

        var skill = new Berserk(new Mock<ILogger<Berserk>>().Object, randomManagerMock.Object, auraManagerMock.Object);
        var actionInput = BuildActionInput<Berserk>(userMock.Object, "berserk");
        var skillActionInput = new SkillActionInput(actionInput, new AbilityDefinition(skill.GetType(), []), userMock.Object);

        var result = skill.Setup(skillActionInput);

        Assert.AreEqual("You're feeling to mellow to berserk.", result);
    }

    [TestMethod]
    public void NoMana()
    {
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IAuraManager> auraManagerMock = new();

        Mock<IPlayableCharacter> userMock = new();
        Mock<IRoom> roomMock = new();
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        userMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
        userMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
        roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());

        var skill = new Berserk(new Mock<ILogger<Berserk>>().Object, randomManagerMock.Object, auraManagerMock.Object);
        var actionInput = BuildActionInput<Berserk>(userMock.Object, "berserk");
        var skillActionInput = new SkillActionInput(actionInput, new AbilityDefinition(skill.GetType(), []), userMock.Object);

        var result = skill.Setup(skillActionInput);

        Assert.AreEqual("You can't use Mana as resource for the moment.", result);
    }

    [TestMethod]
    public void Failed()
    {
        Mock<IRandomManager> randomManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => false);
        Mock<IAuraManager> auraManagerMock = new();

        Mock<IPlayableCharacter> userMock = new();
        Mock<IRoom> roomMock = new();
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        userMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
        userMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
        userMock.SetupGet(x => x.CurrentResourceKinds).Returns([ResourceKinds.Mana]); // has mana
        userMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(1000); // enough mana
        userMock.Setup(x => x.MaxResource(It.Is<ResourceKinds>(rk => rk == ResourceKinds.HitPoints))).Returns(1000);
        roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());

        var skill = new Berserk(new Mock<ILogger<Berserk>>().Object, randomManagerMock.Object, auraManagerMock.Object);
        var actionInput = BuildActionInput<Berserk>(userMock.Object, "berserk");
        var skillActionInput = new SkillActionInput(actionInput, new AbilityDefinition(skill.GetType(), []), userMock.Object);

        var result = skill.Setup(skillActionInput);
        skill.Execute();

        Assert.IsNull(result);
        userMock.Verify(x => x.Send("Your pulse speeds up, but nothing happens.", It.IsAny<object[]>()), Times.Once);
    }

    [TestMethod]
    public void NoEnoughMana()
    {
        Mock<IRandomManager> randomManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
        Mock<IAuraManager> auraManagerMock = new();

        Mock<IPlayableCharacter> userMock = new();
        Mock<IRoom> roomMock = new();
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        userMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
        userMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
        userMock.SetupGet(x => x.CurrentResourceKinds).Returns([ResourceKinds.Mana]); // has mana
        userMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(10); // NOT enough mana
        userMock.Setup(x => x.MaxResource(It.Is<ResourceKinds>(rk => rk == ResourceKinds.HitPoints))).Returns(1000);
        roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());

        var skill = new Berserk(new Mock<ILogger<Berserk>>().Object, randomManagerMock.Object, auraManagerMock.Object);
        var actionInput = BuildActionInput<Berserk>(userMock.Object, "berserk");
        var skillActionInput = new SkillActionInput(actionInput, new AbilityDefinition(skill.GetType(), []), userMock.Object);

        var result = skill.Setup(skillActionInput);

        Assert.AreEqual("You don't have enough Mana.", result);
    }

    [TestMethod]
    public void Success()
    {
        Mock<IRandomManager> randomManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
        Mock<IAuraManager> auraManagerMock = new();

        Mock<IPlayableCharacter> userMock = new();
        Mock<IRoom> roomMock = new();
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        userMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
        userMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
        userMock.SetupGet(x => x.CurrentResourceKinds).Returns([ResourceKinds.Mana]); // has mana
        userMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(1000); // enough mana
        userMock.Setup(x => x.MaxResource(It.Is<ResourceKinds>(rk => rk == ResourceKinds.HitPoints))).Returns(1000);
        roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());

        var skill = new Berserk(new Mock<ILogger<Berserk>>().Object, randomManagerMock.Object, auraManagerMock.Object);
        var actionInput = BuildActionInput<Berserk>(userMock.Object, "berserk");
        var skillActionInput = new SkillActionInput(actionInput, new AbilityDefinition(skill.GetType(), []), userMock.Object);

        var result = skill.Setup(skillActionInput);
        skill.Execute();

        Assert.IsNull(result);
        userMock.Verify(x => x.Send("Your pulse races as you are consumed by rage!", It.IsAny<object[]>()), Times.Once);
    }

    [TestMethod]
    public void UseWithCommand_Guards()
    {
        Mock<IRandomManager> randomManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
        Mock<IAuraManager> auraManagerMock = new();

        Mock<IPlayableCharacter> userMock = new();
        Mock<IRoom> roomMock = new();
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        userMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
        userMock.Setup(x => x.GetAbilityLearnedAndPercentage(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
        userMock.SetupGet(x => x.CurrentResourceKinds).Returns([ResourceKinds.Mana]); // has mana
        userMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(1000); // enough mana
        userMock.Setup(x => x.MaxResource(It.Is<ResourceKinds>(rk => rk == ResourceKinds.HitPoints))).Returns(1000);
        roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());

        var actionInput = BuildActionInput<Berserk>(userMock.Object, "berserk");
        var skill = new Berserk(new Mock<ILogger<Berserk>>().Object, randomManagerMock.Object, auraManagerMock.Object);
        var result = skill.Guards(actionInput);
        skill.Execute(actionInput);

        Assert.IsNull(result);
        userMock.Verify(x => x.Send("Your pulse races as you are consumed by rage!", It.IsAny<object[]>()), Times.Once);
    }
}
