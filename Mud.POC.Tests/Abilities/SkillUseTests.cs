using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.POC.Abilities;
using Mud.Server.Common;
using Mud.Server.Input;
using Mud.Server.Random;
using System.Linq;

namespace Mud.POC.Tests.Abilities
{
    [TestClass]
    public class SkillUseTests : AbilityTestBase
    {
        [TestMethod]
        public void NullAbility_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            IAbility ability = abilityManager["pouet"]; // doesnt exist
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = ability, Learned = 1, Level = 20 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("pouet");
            UseResults result = abilityManager.Use(ability, characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(UseResults.Error, result);
        }

        [TestMethod]
        public void Passive_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            IAbility ability = abilityManager["Whip"];
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = ability, Learned = 1, Level = 20 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("whip");
            UseResults result = abilityManager.Use(ability, characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(UseResults.Error, result);
        }

        [TestMethod]
        public void Spell_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            IAbility ability = abilityManager["Mass invis"];
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = ability, Learned = 1, Level = 20 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Mass invis");
            UseResults result = abilityManager.Use(ability, characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(UseResults.Error, result);
        }

        [TestMethod]
        public void Target_CharacterSelf_NoTargetSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            Skills.RandomManager = randomManagerMock.Object;
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            IAbility ability = abilityManager["Berserk"];
            var characterMock = new Mock<IPlayableCharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.HitPoints).Returns(1000);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = ability, Learned = 1, Level = 20 } });
            characterMock.Setup(x => x.LearnedAbility(It.IsAny<IAbility>())).Returns<IAbility>(x => 1);
            characterMock.Setup(x => x.CurrentAttributes(It.IsAny<CharacterAttributes>())).Returns<CharacterAttributes>(x => 20);

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Berserk");
            UseResults result = abilityManager.Use(ability, characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(UseResults.Ok, result);
        }

        [TestMethod]
        public void Target_CharacterSelf_TargetSelfSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            Skills.RandomManager = randomManagerMock.Object;
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            IAbility ability = abilityManager["Berserk"];
            var characterMock = new Mock<IPlayableCharacter>();
            characterMock.SetupGet(x => x.Name).Returns("mob1");
            characterMock.SetupGet(x => x.Keywords).Returns(new[] { "mob1"});
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.HitPoints).Returns(1000);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = ability, Learned = 1, Level = 20 } });
            characterMock.Setup(x => x.LearnedAbility(It.IsAny<IAbility>())).Returns<IAbility>(x => 1);
            characterMock.Setup(x => x.CurrentAttributes(It.IsAny<CharacterAttributes>())).Returns<CharacterAttributes>(x => 20);
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { characterMock.Object });
            characterMock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Berserk mob1");
            UseResults result = abilityManager.Use(ability, characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(UseResults.Ok, result);
        }

        [TestMethod]
        public void Target_CharacterSelf_TargetNotFoundSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            Skills.RandomManager = randomManagerMock.Object;
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            IAbility ability = abilityManager["Berserk"];
            var characterMock = new Mock<IPlayableCharacter>();
            characterMock.SetupGet(x => x.Name).Returns("mob1");
            characterMock.SetupGet(x => x.Keywords).Returns(new[] { "mob1" });
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.HitPoints).Returns(1000);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = ability, Learned = 1, Level = 20 } });
            characterMock.Setup(x => x.LearnedAbility(It.IsAny<IAbility>())).Returns<IAbility>(x => 1);
            characterMock.Setup(x => x.CurrentAttributes(It.IsAny<CharacterAttributes>())).Returns<CharacterAttributes>(x => 20);
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { characterMock.Object });
            characterMock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Berserk mob2");
            UseResults result = abilityManager.Use(ability, characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(UseResults.InvalidTarget, result);
        }

        [TestMethod]
        public void Target_CharacterSelf_TargetSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            Skills.RandomManager = randomManagerMock.Object;
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            IAbility ability = abilityManager["Berserk"];
            var characterMock = new Mock<IPlayableCharacter>();
            characterMock.SetupGet(x => x.Name).Returns("mob1");
            characterMock.SetupGet(x => x.Keywords).Returns(new[] { "mob1" });
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.HitPoints).Returns(1000);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = ability, Learned = 1, Level = 20 } });
            characterMock.Setup(x => x.LearnedAbility(It.IsAny<IAbility>())).Returns<IAbility>(x => 1);
            characterMock.Setup(x => x.CurrentAttributes(It.IsAny<CharacterAttributes>())).Returns<CharacterAttributes>(x => 20);
            var mob2Mock = new Mock<ICharacter>();
            mob2Mock.SetupGet(x => x.Name).Returns("mob2");
            mob2Mock.SetupGet(x => x.Keywords).Returns(new[] { "mob2" });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { characterMock.Object, mob2Mock.Object });
            characterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            mob2Mock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Berserk mob2");
            UseResults result = abilityManager.Use(ability, characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(UseResults.InvalidTarget, result);
        }

        [TestMethod]
        public void NotKnown_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            Skills.RandomManager = randomManagerMock.Object;
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            IAbility ability = abilityManager["Berserk"];
            var characterMock = new Mock<IPlayableCharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.HitPoints).Returns(1000);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = ability, Learned = 0, Level = 20 } }); // Learned:0 -> not known
            characterMock.Setup(x => x.LearnedAbility(It.IsAny<IAbility>())).Returns<IAbility>(x => 0); // Learned:0 -> not known
            characterMock.Setup(x => x.CurrentAttributes(It.IsAny<CharacterAttributes>())).Returns<CharacterAttributes>(x => 20);
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { characterMock.Object });
            characterMock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Berserk");
            UseResults result = abilityManager.Use(ability, characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(UseResults.NotKnown, result);
        }
    }
}
