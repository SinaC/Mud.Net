using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Common;
using Mud.POC.Abilities2;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Rom24Skills;
using Mud.Server.Common;
using Mud.Server.Random;

namespace Mud.POC.Tests.Abilities2
{
    [TestClass]
    public class BerserkTests : TestBase
    {
        [TestMethod]
        public void PcNotKnown()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();

            Mock<IPlayableCharacter> userMock = new Mock<IPlayableCharacter>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());

            Berserk skill = new Berserk(randomManagerMock.Object, auraManagerMock.Object);
            var actionInput = BuildActionInput(userMock.Object, "berserk");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);

            Assert.AreEqual("You turn red in the face, but nothing happens.", result);
        }

        [TestMethod]
        public void NpcNotKnownNoOffensiveFlags()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();

            Mock<INonPlayableCharacter> userMock = new Mock<INonPlayableCharacter>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());

            Berserk skill = new Berserk(randomManagerMock.Object, auraManagerMock.Object);
            var actionInput = BuildActionInput(userMock.Object, "berserk");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);

            Assert.AreEqual("You turn red in the face, but nothing happens.", result);
        }

        [TestMethod]
        public void NpcNotKnownBerserkOffensiveFlags()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();

            Mock<INonPlayableCharacter> userMock = new Mock<INonPlayableCharacter>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.SetupGet(x => x.OffensiveFlags).Returns(OffensiveFlags.Berserk);
            userMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());

            Berserk skill = new Berserk(randomManagerMock.Object, auraManagerMock.Object);
            var actionInput = BuildActionInput(userMock.Object, "berserk");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);

            Assert.AreEqual("You turn red in the face, but nothing happens.", result);
        }

        [TestMethod]
        public void HasBerserkFlags()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();

            Mock<IPlayableCharacter> userMock = new Mock<IPlayableCharacter>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(abilityName => (100, new AbilityLearned { Name = abilityName }));
            userMock.SetupGet(x => x.CharacterFlags).Returns(CharacterFlags.Berserk);
            userMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());

            Berserk skill = new Berserk(randomManagerMock.Object, auraManagerMock.Object);
            var actionInput = BuildActionInput(userMock.Object, "berserk");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);

            Assert.AreEqual("You get a little madder.", result);
        }

        [TestMethod]
        public void AffectedByBerserk()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();

            Mock<IPlayableCharacter> userMock = new Mock<IPlayableCharacter>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(abilityName => (100, new AbilityLearned { Name = abilityName }));
            userMock.Setup(x => x.GetAura(Berserk.SkillName)).Returns<string>(_ => new Mock<IAura>().Object);
            userMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());

            Berserk skill = new Berserk(randomManagerMock.Object, auraManagerMock.Object);
            var actionInput = BuildActionInput(userMock.Object, "berserk");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);

            Assert.AreEqual("You get a little madder.", result);
        }

        [TestMethod]
        public void HasCalmFlag()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();

            Mock<IPlayableCharacter> userMock = new Mock<IPlayableCharacter>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(abilityName => (100, new AbilityLearned { Name = abilityName }));
            userMock.SetupGet(x => x.CharacterFlags).Returns(CharacterFlags.Calm);
            userMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());

            Berserk skill = new Berserk(randomManagerMock.Object, auraManagerMock.Object);
            var actionInput = BuildActionInput(userMock.Object, "berserk");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);

            Assert.AreEqual("You're feeling to mellow to berserk.", result);
        }

        [TestMethod]
        public void NoMana()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();

            Mock<IPlayableCharacter> userMock = new Mock<IPlayableCharacter>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(abilityName => (100, new AbilityLearned { Name = abilityName }));
            userMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(0);
            roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());

            Berserk skill = new Berserk(randomManagerMock.Object, auraManagerMock.Object);
            var actionInput = BuildActionInput(userMock.Object, "berserk");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);

            Assert.AreEqual("You can't get up enough energy.", result);
        }

        [TestMethod]
        public void Failed()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => false);
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();

            Mock<IPlayableCharacter> userMock = new Mock<IPlayableCharacter>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(abilityName => (100, new AbilityLearned { Name = abilityName }));
            userMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            userMock.SetupGet(x => x.MaxHitPoints).Returns(1000);
            roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());

            Berserk skill = new Berserk(randomManagerMock.Object, auraManagerMock.Object);
            var actionInput = BuildActionInput(userMock.Object, "berserk");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);
            skill.Execute();

            Assert.IsNull(result);
            userMock.Verify(x => x.Send("Your pulse speeds up, but nothing happens.", It.IsAny<object[]>()), Times.Once);
        }

        [TestMethod]
        public void Success()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();

            Mock<IPlayableCharacter> userMock = new Mock<IPlayableCharacter>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(abilityName => (100, new AbilityLearned { Name = abilityName }));
            userMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            userMock.SetupGet(x => x.MaxHitPoints).Returns(1000);
            roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());

            Berserk skill = new Berserk(randomManagerMock.Object, auraManagerMock.Object);
            var actionInput = BuildActionInput(userMock.Object, "berserk");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(skill.GetType()), userMock.Object);

            string result = skill.Setup(skillActionInput);
            skill.Execute();

            Assert.IsNull(result);
            userMock.Verify(x => x.Send("Your pulse races as you are consumed by rage!", It.IsAny<object[]>()), Times.Once);
        }

        [TestMethod]
        public void UseWithCommand_Guards()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();

            Mock<IPlayableCharacter> userMock = new Mock<IPlayableCharacter>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(abilityName => (100, new AbilityLearned { Name = abilityName }));
            userMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            userMock.SetupGet(x => x.MaxHitPoints).Returns(1000);
            roomMock.SetupGet(x => x.People).Returns(userMock.Object.Yield());

            var actionInput = new ActionInput(userMock.Object, "berserk");

            Berserk skill = new Berserk(randomManagerMock.Object, auraManagerMock.Object);
            var result = skill.Guards(actionInput);
            skill.Execute(actionInput);

            Assert.IsNull(result);
            userMock.Verify(x => x.Send("Your pulse races as you are consumed by rage!", It.IsAny<object[]>()), Times.Once);
        }
    }
}
