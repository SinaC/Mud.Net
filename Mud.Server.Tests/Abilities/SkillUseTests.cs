using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Domain;
using Mud.Server.Abilities;
using Mud.Server.Common;
using Mud.Server.Input;
using System;

namespace Mud.Server.Tests.Abilities
{
    [TestClass]
    public class SkillUseTests : AbilityTestBase
    {
        [TestMethod]
        public void NullAbility_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object, null, null, null, null);
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
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object, null, null, null, null);
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
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object, null, null, null, null);
            IAbility ability = abilityManager["Mass invis"];
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = ability, Learned = 1, Level = 20 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Mass invis");
            UseResults result = abilityManager.Use(ability, characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(UseResults.Error, result);
        }

        // CharacterSelf doesn't care about target
        [TestMethod]
        public void Target_CharacterSelf_NoTargetSpecified_Test()
        {
            var worldMock = new Mock<IWorld>();
            worldMock.Setup(x => x.AddAura(It.IsAny<IEntity>(), It.IsAny<IAbility>(), It.IsAny<IEntity>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()))
                .Returns<IEntity, IAbility, IEntity, int, TimeSpan, AuraFlags, bool, IAffect[]>((target, ability, source, level, duration, flags, recompute, affects) => new Mock<IAura>().Object);
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object, null, worldMock.Object, null, null);
            IAbility berserk = abilityManager["Berserk"];
            var characterMock = new Mock<IPlayableCharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.HitPoints).Returns(1000);
            characterMock.SetupGet(x => x.MaxHitPoints).Returns(1000);
            characterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(1000);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = berserk, Learned = 1, Level = 20 } });
            characterMock.Setup(x => x.GetLearnInfo(It.IsAny<IAbility>())).Returns<IAbility>(x => (1, new KnownAbility { Ability = berserk, Learned = 1, Level = 20 }));
            characterMock.SetupGet(x => x[It.IsAny<CharacterAttributes>()]).Returns(20);

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Berserk");
            UseResults result = abilityManager.Use(berserk, characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(UseResults.Ok, result);
        }

        [TestMethod]
        public void Target_CharacterSelf_TargetSelfSpecified_Test()
        {
            var worldMock = new Mock<IWorld>();
            worldMock.Setup(x => x.AddAura(It.IsAny<IEntity>(), It.IsAny<IAbility>(), It.IsAny<IEntity>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()))
                .Returns<IEntity, IAbility, IEntity, int, TimeSpan, AuraFlags, bool, IAffect[]>((target, ability, source, level, duration, flags, recompute, affects) => new Mock<IAura>().Object);
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object, null, worldMock.Object, null, null);
            IAbility berserk = abilityManager["Berserk"];
            var characterMock = new Mock<IPlayableCharacter>();
            characterMock.SetupGet(x => x.Name).Returns("mob1");
            characterMock.SetupGet(x => x.Keywords).Returns(new[] { "mob1"});
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.HitPoints).Returns(1000);
            characterMock.SetupGet(x => x.MaxHitPoints).Returns(1000);
            characterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(1000);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = berserk, Learned = 1, Level = 20 } });
            characterMock.Setup(x => x.GetLearnInfo(It.IsAny<IAbility>())).Returns<IAbility>(x => (1, new KnownAbility { Ability = berserk, Learned = 1, Level = 20 }));
            characterMock.SetupGet(x => x[It.IsAny<CharacterAttributes>()]).Returns(20);
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { characterMock.Object });
            characterMock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Berserk mob1");
            UseResults result = abilityManager.Use(berserk, characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(UseResults.Ok, result);
        }

        [TestMethod]
        public void Target_CharacterSelf_TargetNotFoundSpecified_Test()
        {
            var worldMock = new Mock<IWorld>();
            worldMock.Setup(x => x.AddAura(It.IsAny<IEntity>(), It.IsAny<IAbility>(), It.IsAny<IEntity>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()))
                .Returns<IEntity, IAbility, IEntity, int, TimeSpan, AuraFlags, bool, IAffect[]>((target, ability, source, level, duration, flags, recompute, affects) => new Mock<IAura>().Object);
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object, null, worldMock.Object, null, null);
            IAbility berserk = abilityManager["Berserk"];
            var characterMock = new Mock<IPlayableCharacter>();
            characterMock.SetupGet(x => x.Name).Returns("mob1");
            characterMock.SetupGet(x => x.Keywords).Returns(new[] { "mob1" });
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.HitPoints).Returns(1000);
            characterMock.SetupGet(x => x.MaxHitPoints).Returns(1000);
            characterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(1000);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = berserk, Learned = 1, Level = 20 } });
            characterMock.Setup(x => x.GetLearnInfo(It.IsAny<IAbility>())).Returns<IAbility>(x => (1, new KnownAbility { Ability = berserk, Learned = 1, Level = 20 }));
            characterMock.SetupGet(x => x[It.IsAny<CharacterAttributes>()]).Returns(20);
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { characterMock.Object });
            characterMock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Berserk mob2");
            UseResults result = abilityManager.Use(berserk, characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(UseResults.Ok, result);
        }

        [TestMethod]
        public void Target_CharacterSelf_TargetSpecified_Test()
        {
            var worldMock = new Mock<IWorld>();
            worldMock.Setup(x => x.AddAura(It.IsAny<IEntity>(), It.IsAny<IAbility>(), It.IsAny<IEntity>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()))
                .Returns<IEntity, IAbility, IEntity, int, TimeSpan, AuraFlags, bool, IAffect[]>((target, ability, source, level, duration, flags, recompute, affects) => new Mock<IAura>().Object);
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object, null, worldMock.Object, null, null);
            IAbility berserk = abilityManager["Berserk"];
            var characterMock = new Mock<IPlayableCharacter>();
            characterMock.SetupGet(x => x.Name).Returns("mob1");
            characterMock.SetupGet(x => x.Keywords).Returns(new[] { "mob1" });
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.HitPoints).Returns(1000);
            characterMock.SetupGet(x => x.MaxHitPoints).Returns(1000);
            characterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(1000);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = berserk, Learned = 1, Level = 20 } });
            characterMock.Setup(x => x.GetLearnInfo(It.IsAny<IAbility>())).Returns<IAbility>(x => (1, new KnownAbility { Ability = berserk, Learned = 1, Level = 20 }));
            characterMock.SetupGet(x => x[It.IsAny<CharacterAttributes>()]).Returns(20);
            var mob2Mock = new Mock<ICharacter>();
            mob2Mock.SetupGet(x => x.Name).Returns("mob2");
            mob2Mock.SetupGet(x => x.Keywords).Returns(new[] { "mob2" });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { characterMock.Object, mob2Mock.Object });
            characterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            mob2Mock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Berserk mob2");
            UseResults result = abilityManager.Use(berserk, characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(UseResults.Ok, result);
        }

        [TestMethod]
        public void NotKnown_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object, null, null, null, null);
            IAbility berserk = abilityManager["Berserk"];
            var characterMock = new Mock<IPlayableCharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.HitPoints).Returns(1000);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = berserk, Learned = 0, Level = 20 } }); // Learned:0 -> not known
            characterMock.Setup(x => x.GetLearnInfo(It.IsAny<IAbility>())).Returns<IAbility>(x => (0, new KnownAbility { Ability = berserk, Learned = 0, Level = 20 }));
            characterMock.SetupGet(x => x[It.IsAny<CharacterAttributes>()]).Returns(20);
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { characterMock.Object });
            characterMock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Berserk");
            UseResults result = abilityManager.Use(berserk, characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(UseResults.NotKnown, result);
        }
    }
}
