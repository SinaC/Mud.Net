using Moq;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Character.Ability;
using Mud.Server.Flags;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Room;
using Mud.Server.Random;
using Mud.Server.Rom24.Spells;

namespace Mud.Server.Rom24.Tests.Abilities;

[TestClass]
public class AcidBlastTests : AbilityTestBase
{
    protected override void RegisterAdditionalDependencies(Mock<IServiceProvider> serviceProviderMock)
    {
        // SpellBase needs IRandomManager to check if cast loses his/her/its concentration
        Mock<IRandomManager> randomManagerMock = new();
        randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(x => true);
        randomManagerMock.Setup(x => x.Dice(It.IsAny<int>(), It.IsAny<int>())).Returns<int, int>((count, value) => count * value);
        serviceProviderMock.Setup(x => x.GetService(typeof(AcidBlast)))
            .Returns(new AcidBlast(randomManagerMock.Object));
    }

    [TestMethod]
    public void Cast_Sleeping()
    {
        Mock<IWiznet> wiznetMock = new();
        Mock<IRoom> roomMock = new();
        Mock<IPlayableCharacter> casterMock = new();
        Mock<INonPlayableCharacter> victimMock = new();

        var acidBlastLearned = BuildAbilityLearned("Acid Blast");
        var abilityManager = new AbilityManager(_serviceProvider, new AssemblyHelper());
        //
        casterMock.SetupGet(x => x.Name).Returns("caster");
        casterMock.SetupGet(x => x.Level).Returns(50);
        casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns(acidBlastLearned);
        casterMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags(_serviceProvider));
        casterMock.SetupGet(x => x.Position).Returns(Positions.Sleeping);
        victimMock.SetupGet(x => x.Name).Returns("target");
        victimMock.SetupGet(x => x.Keywords).Returns(["target"]);
        victimMock.Setup(x => x.SavesSpell(It.IsAny<int>(), It.IsAny<SchoolTypes>())).Returns<int, SchoolTypes>((level, damageType) => false);
        roomMock.SetupGet(x => x.People).Returns([casterMock.Object, victimMock.Object]);
        casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        //
        Cast cast = new(abilityManager);
        var actionInput = BuildActionInput<Cast>(casterMock.Object, "Cast 'Acid Blast'");
        var result = cast.Guards(actionInput);

        Assert.AreEqual("In your dreams, or what?", result);
    }

    [TestMethod]
    public void Cast_Resting()
    {
        Mock<IWiznet> wiznetMock = new();
        Mock<IRoom> roomMock = new();
        Mock<IPlayableCharacter> casterMock = new();
        Mock<INonPlayableCharacter> victimMock = new();

        var acidBlastLearned = BuildAbilityLearned("Acid Blast");
        var abilityManager = new AbilityManager(_serviceProvider, new AssemblyHelper());
        //
        casterMock.SetupGet(x => x.Name).Returns("caster");
        casterMock.SetupGet(x => x.Level).Returns(50);
        casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns(acidBlastLearned);
        casterMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags(_serviceProvider));
        casterMock.SetupGet(x => x.Position).Returns(Positions.Resting);
        victimMock.SetupGet(x => x.Name).Returns("target");
        victimMock.SetupGet(x => x.Keywords).Returns(["target"]);
        victimMock.Setup(x => x.SavesSpell(It.IsAny<int>(), It.IsAny<SchoolTypes>())).Returns<int, SchoolTypes>((level, damageType) => false);
        roomMock.SetupGet(x => x.People).Returns([casterMock.Object, victimMock.Object]);
        casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        //
        Cast cast = new(abilityManager);
        var actionInput = BuildActionInput<Cast>(casterMock.Object, "Cast 'Acid Blast'");
        var result = cast.Guards(actionInput);

        Assert.AreEqual("Nah... You feel too relaxed...", result);
    }

    [TestMethod]
    public void Cast_Sitting()
    {
        Mock<IWiznet> wiznetMock = new();
        Mock<IRoom> roomMock = new();
        Mock<IPlayableCharacter> casterMock = new();
        Mock<INonPlayableCharacter> victimMock = new();

        var acidBlastLearned = BuildAbilityLearned("Acid Blast");
        var abilityManager = new AbilityManager(_serviceProvider, new AssemblyHelper());
        //
        casterMock.SetupGet(x => x.Name).Returns("caster");
        casterMock.SetupGet(x => x.Level).Returns(50);
        casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns(acidBlastLearned);
        casterMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags(_serviceProvider));
        casterMock.SetupGet(x => x.Position).Returns(Positions.Sitting);
        victimMock.SetupGet(x => x.Name).Returns("target");
        victimMock.SetupGet(x => x.Keywords).Returns(["target"]);
        victimMock.Setup(x => x.SavesSpell(It.IsAny<int>(), It.IsAny<SchoolTypes>())).Returns<int, SchoolTypes>((level, damageType) => false);
        roomMock.SetupGet(x => x.People).Returns([casterMock.Object, victimMock.Object]);
        casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        //
        Cast cast = new(abilityManager);
        var actionInput = BuildActionInput<Cast>(casterMock.Object, "Cast 'Acid Blast'");
        var result = cast.Guards(actionInput);

        Assert.AreEqual("Better stand up first.", result);
    }

    [TestMethod]
    public void Cast_Stunned()
    {
        Mock<IWiznet> wiznetMock = new();
        Mock<IRoom> roomMock = new();
        Mock<IPlayableCharacter> casterMock = new();
        Mock<INonPlayableCharacter> victimMock = new();

        var acidBlastLearned = BuildAbilityLearned("Acid Blast");
        var abilityManager = new AbilityManager(_serviceProvider, new AssemblyHelper());
        //
        casterMock.SetupGet(x => x.Name).Returns("caster");
        casterMock.SetupGet(x => x.Level).Returns(50);
        casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns(acidBlastLearned);
        casterMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags(_serviceProvider));
        casterMock.SetupGet(x => x.Position).Returns(Positions.Standing);
        casterMock.SetupGet(x => x.Stunned).Returns(10);
        victimMock.SetupGet(x => x.Name).Returns("target");
        victimMock.SetupGet(x => x.Keywords).Returns(["target"]);
        victimMock.Setup(x => x.SavesSpell(It.IsAny<int>(), It.IsAny<SchoolTypes>())).Returns<int, SchoolTypes>((level, damageType) => false);
        roomMock.SetupGet(x => x.People).Returns([casterMock.Object, victimMock.Object]);
        casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        //
        Cast cast = new(abilityManager);
        var actionInput = BuildActionInput<Cast>(casterMock.Object, "Cast 'Acid Blast'");
        var result = cast.Guards(actionInput);

        Assert.AreEqual("You're still a little woozy.", result);
    }

    [TestMethod]
    public void Cast_Guards_NoTarget()
    {
        Mock<IWiznet> wiznetMock = new();
        Mock<IRoom> roomMock = new();
        Mock<IPlayableCharacter> casterMock = new();
        Mock<INonPlayableCharacter> victimMock = new();

        var acidBlastLearned = BuildAbilityLearned("Acid Blast");
        var abilityManager = new AbilityManager(_serviceProvider, new AssemblyHelper());
        //
        casterMock.SetupGet(x => x.Name).Returns("caster");
        casterMock.SetupGet(x => x.Level).Returns(50);
        casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns(acidBlastLearned);
        casterMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags(_serviceProvider));
        casterMock.SetupGet(x => x.Position).Returns(Positions.Standing);
        victimMock.SetupGet(x => x.Name).Returns("target");
        victimMock.SetupGet(x => x.Keywords).Returns(["target"]);
        victimMock.Setup(x => x.SavesSpell(It.IsAny<int>(), It.IsAny<SchoolTypes>())).Returns<int, SchoolTypes>((level, damageType) => false);
        roomMock.SetupGet(x => x.People).Returns([casterMock.Object, victimMock.Object]);
        casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        //
        Cast cast = new(abilityManager);
        var actionInput = BuildActionInput<Cast>(casterMock.Object, "Cast 'Acid Blast'");
        var result = cast.Guards(actionInput);

        Assert.AreEqual("Cast the spell on whom?", result);
    }
    
    [TestMethod]
    public void Cast_Guards_TargetNotFound()
    {
        Mock<IWiznet> wiznetMock = new();
        Mock<IRoom> roomMock = new();
        Mock<IPlayableCharacter> casterMock = new();
        Mock<INonPlayableCharacter> victimMock = new();

        var acidBlastLearned = BuildAbilityLearned("Acid Blast");
        var abilityManager = new AbilityManager(_serviceProvider, new AssemblyHelper());
        //
        casterMock.SetupGet(x => x.Name).Returns("caster");
        casterMock.SetupGet(x => x.Level).Returns(50);
        casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns(acidBlastLearned);
        casterMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags(_serviceProvider));
        casterMock.SetupGet(x => x.Position).Returns(Positions.Standing);
        victimMock.SetupGet(x => x.Name).Returns("target");
        victimMock.SetupGet(x => x.Keywords).Returns(["target"]);
        victimMock.Setup(x => x.SavesSpell(It.IsAny<int>(), It.IsAny<SchoolTypes>())).Returns<int, SchoolTypes>((level, damageType) => false);
        roomMock.SetupGet(x => x.People).Returns([casterMock.Object, victimMock.Object]);
        casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        //
        Cast cast = new(abilityManager);
        var actionInput = BuildActionInput<Cast>(casterMock.Object, "Cast 'Acid Blast' pouet");
        var result = cast.Guards(actionInput);

        Assert.AreEqual("They aren't here.", result);
    }

    [TestMethod]
    public void Cast_Guards_SafeTarget()
    {
        Mock<IWiznet> wiznetMock = new();
        Mock<IRoom> roomMock = new();
        Mock<IPlayableCharacter> casterMock = new();
        Mock<INonPlayableCharacter> victimMock = new();

        var acidBlastLearned = BuildAbilityLearned("Acid Blast");
        var abilityManager = new AbilityManager(_serviceProvider, new AssemblyHelper());
        //
        casterMock.SetupGet(x => x.Name).Returns("caster");
        casterMock.SetupGet(x => x.Level).Returns(50);
        casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns(acidBlastLearned);
        casterMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags(_serviceProvider));
        casterMock.SetupGet(x => x.Position).Returns(Positions.Standing);
        victimMock.SetupGet(x => x.Name).Returns("target");
        victimMock.SetupGet(x => x.Keywords).Returns(["target"]);
        victimMock.Setup(x => x.SavesSpell(It.IsAny<int>(), It.IsAny<SchoolTypes>())).Returns<int, SchoolTypes>((level, damageType) => false);
        victimMock.Setup(x => x.IsSafe(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => "Not on that victim.");
        roomMock.SetupGet(x => x.People).Returns([casterMock.Object, victimMock.Object]);
        casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        //
        Cast cast = new(abilityManager);
        var actionInput = BuildActionInput<Cast>(casterMock.Object, "Cast 'Acid Blast' target");
        var result = cast.Guards(actionInput);

        Assert.AreEqual("Not on that victim.", result);
    }

    [TestMethod]
    public void Cast_Guards_NotOnMaster()
    {
        Mock<IWiznet> wiznetMock = new();
        Mock<IRoom> roomMock = new();
        Mock<INonPlayableCharacter> casterMock = new();
        Mock<IPlayableCharacter> victimMock = new();

        var acidBlastLearned = BuildAbilityLearned("Acid Blast");
        var abilityManager = new AbilityManager(_serviceProvider, new AssemblyHelper());
        //
        casterMock.SetupGet(x => x.Name).Returns("caster");
        casterMock.SetupGet(x => x.Level).Returns(50);
        casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns(acidBlastLearned);
        casterMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags(_serviceProvider, "Charm"));
        casterMock.SetupGet(x => x.Master).Returns(victimMock.Object);
        casterMock.SetupGet(x => x.Position).Returns(Positions.Standing);
        victimMock.SetupGet(x => x.Name).Returns("target");
        victimMock.SetupGet(x => x.Keywords).Returns(["target"]);
        victimMock.Setup(x => x.SavesSpell(It.IsAny<int>(), It.IsAny<SchoolTypes>())).Returns<int, SchoolTypes>((level, damageType) => false);
        victimMock.Setup(x => x.IsSafe(It.IsAny<ICharacter>())).Returns<ICharacter>(_ => null);
        roomMock.SetupGet(x => x.People).Returns([casterMock.Object, victimMock.Object]);
        casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        //
        Cast cast = new(abilityManager);
        var actionInput = BuildActionInput<Cast>(casterMock.Object, "Cast 'Acid Blast' target");
        var result = cast.Guards(actionInput);

        Assert.AreEqual("You can't do that on your own follower.", result);
    }

    [TestMethod]
    public void Execute_Spell()
    {
        Mock<IWiznet> wiznetMock = new();
        Mock<IRoom> roomMock = new();
        Mock<IPlayableCharacter> casterMock = new();
        Mock<INonPlayableCharacter> victimMock = new();

        var acidBlastLearned = BuildAbilityLearned("Acid Blast");
        var abilityManager = new AbilityManager(_serviceProvider, new AssemblyHelper());
        //
        casterMock.SetupGet(x => x.Name).Returns("caster");
        casterMock.SetupGet(x => x.Level).Returns(50);
        casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns(acidBlastLearned);
        casterMock.Setup(x => x.GetAbilityLearnedInfo(It.IsAny<string>())).Returns<string>(x => (100, acidBlastLearned));
        casterMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags(_serviceProvider));
        casterMock.SetupGet(x => x.Position).Returns(Positions.Standing);
        victimMock.SetupGet(x => x.Name).Returns("target");
        victimMock.SetupGet(x => x.Keywords).Returns(["target"]);
        victimMock.Setup(x => x.SavesSpell(It.IsAny<int>(), It.IsAny<SchoolTypes>())).Returns<int, SchoolTypes>((level, damageType) => false);
        roomMock.SetupGet(x => x.People).Returns([casterMock.Object, victimMock.Object]);
        casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        //
        Cast cast = new(abilityManager);
        var actionInput = BuildActionInput<Cast>(casterMock.Object, "Cast 'Acid Blast' target");
        cast.Guards(actionInput);
        cast.Execute(actionInput);

        victimMock.Verify(x => x.AbilityDamage(casterMock.Object, 50 * 12/*level * 12 = acid blast damage formula*/, It.IsAny<SchoolTypes>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
    }
}
