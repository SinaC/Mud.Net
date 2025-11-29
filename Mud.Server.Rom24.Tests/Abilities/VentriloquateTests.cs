using Microsoft.Extensions.Logging;
using Moq;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Room;
using Mud.Server.Random;
using Mud.Server.Rom24.Spells;

namespace Mud.Server.Rom24.Tests.Abilities
{
    [TestClass]
    public class VentriloquateTests : AbilityTestBase
    {
        [TestMethod]
        public void NoTarget() 
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Keywords).Returns("player".Yield());
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityLearnedInfo(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            Ventriloquate spell = new Ventriloquate(new Mock<ILogger<Ventriloquate>>().Object, randomManagerMock.Object, new CommandParser(new Mock<ILogger<CommandParser>>().Object));
            
            var parameters = BuildParameters("");
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(new Mock<ILogger>().Object, spell.GetType()), casterMock.Object, 10, null, parameters);

            var result = spell.Setup(abilityActionInput);

            Assert.AreEqual("Make who saying what?", result);
        }


        [TestMethod]
        public void NothingToSay()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Keywords).Returns("player".Yield());
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityLearnedInfo(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            Ventriloquate spell = new Ventriloquate(new Mock<ILogger<Ventriloquate>>().Object, randomManagerMock.Object, new CommandParser(new Mock<ILogger<CommandParser>>().Object));

            var parameters = BuildParameters("target");
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(new Mock<ILogger>().Object, spell.GetType()), casterMock.Object, 10, null, parameters);

            var result = spell.Setup(abilityActionInput);

            Assert.AreEqual("Make who saying what?", result);
        }

        [TestMethod]
        public void TargetNotFound()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Keywords).Returns("player".Yield());
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityLearnedInfo(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            Ventriloquate spell = new Ventriloquate(new Mock<ILogger<Ventriloquate>>().Object, randomManagerMock.Object, new CommandParser(new Mock<ILogger<CommandParser>>().Object));

            var parameters = BuildParameters("target 'I'm a badass'");
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(new Mock<ILogger>().Object, spell.GetType()), casterMock.Object, 10, null, parameters);

            var result = spell.Setup(abilityActionInput);

            Assert.AreEqual("They aren't here.", result);
        }

        [TestMethod]
        public void InvalidTarget()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Keywords).Returns("player".Yield());
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityLearnedInfo(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            Ventriloquate spell = new Ventriloquate(new Mock<ILogger<Ventriloquate>>().Object, randomManagerMock.Object, new CommandParser(new Mock<ILogger<CommandParser>>().Object));

            var parameters = BuildParameters("player 'I'm a badass'");
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(new Mock<ILogger>().Object, spell.GetType()), casterMock.Object, 10, null, parameters);

            var result = spell.Setup(abilityActionInput);

            Assert.AreEqual("Just say it.", result);
        }

        [TestMethod]
        public void ValidTarget_Quote()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Keywords).Returns("player".Yield());
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityLearnedInfo(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            Mock<ICharacter> targetMock = new Mock<ICharacter>();
            targetMock.SetupGet(x => x.Name).Returns("target");
            targetMock.SetupGet(x => x.Keywords).Returns("target".Yield());
            roomMock.SetupGet(x => x.People).Returns(new[] { casterMock.Object, targetMock.Object });
            Ventriloquate spell = new Ventriloquate(new Mock<ILogger<Ventriloquate>>().Object, randomManagerMock.Object, new CommandParser(new Mock<ILogger<CommandParser>>().Object));

            var parameters = BuildParameters("target 'I'm a badass'");
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(new Mock<ILogger>().Object, spell.GetType()), casterMock.Object, 10, null, parameters);

            var result = spell.Setup(abilityActionInput);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void ValidTarget_NoQuote()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Keywords).Returns("player".Yield());
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityLearnedInfo(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            Mock<ICharacter> targetMock = new Mock<ICharacter>();
            targetMock.SetupGet(x => x.Name).Returns("target");
            targetMock.SetupGet(x => x.Keywords).Returns("target".Yield());
            roomMock.SetupGet(x => x.People).Returns(new[] { casterMock.Object, targetMock.Object });
            Ventriloquate spell = new Ventriloquate(new Mock<ILogger<Ventriloquate>>().Object, randomManagerMock.Object, new CommandParser(new Mock<ILogger<CommandParser>>().Object));

            var parameters = BuildParameters("target I'm a badass");
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(new Mock<ILogger>().Object, spell.GetType()), casterMock.Object, 10, null, parameters);

            var result = spell.Setup(abilityActionInput);

            Assert.IsNull(result);
        }
    }
}
