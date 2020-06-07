﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.POC.Abilities2;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Rom24Spells;
using Mud.Server.Common;
using Mud.Server.Input;
using System.Linq;

namespace Mud.POC.Tests.Abilities2
{
    [TestClass]
    public class VentriloquateTests : TestBase
    {
        [TestMethod]
        public void NoTarget() 
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IWiznet> wiznetMock = new Mock<IWiznet>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityPercentage(It.IsAny<IAbility>())).Returns<IAbility>(x => (100, new AbilityLearned { Name = "Ventriloquate" }));
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            Ventriloquate spell = new Ventriloquate(randomManagerMock.Object, wiznetMock.Object);
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, "", Enumerable.Empty<CommandParameter>().ToArray());

            string result = spell.Guards(abilityActionInput);

            Assert.AreEqual("Make who saying what?", result);
        }


        [TestMethod]
        public void NothingToSay()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IWiznet> wiznetMock = new Mock<IWiznet>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityPercentage(It.IsAny<IAbility>())).Returns<IAbility>(x => (100, new AbilityLearned { Name = "Ventriloquate" }));
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            Ventriloquate spell = new Ventriloquate(randomManagerMock.Object, wiznetMock.Object);
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, "target", new CommandParameter("target", false));

            string result = spell.Guards(abilityActionInput);

            Assert.AreEqual("Make who saying what?", result);
        }

        [TestMethod]
        public void TargetNotFound()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IWiznet> wiznetMock = new Mock<IWiznet>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityPercentage(It.IsAny<IAbility>())).Returns<IAbility>(x => (100, new AbilityLearned { Name = "Ventriloquate" }));
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            Ventriloquate spell = new Ventriloquate(randomManagerMock.Object, wiznetMock.Object);
            var parameters = BuildParameters("target 'I'm a badass'");
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, parameters.rawParameters, parameters.parameters);

            string result = spell.Guards(abilityActionInput);

            Assert.AreEqual("They aren't here.", result);
        }

        [TestMethod]
        public void InvalidTarget()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IWiznet> wiznetMock = new Mock<IWiznet>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityPercentage(It.IsAny<IAbility>())).Returns<IAbility>(x => (100, new AbilityLearned { Name = "Ventriloquate" }));
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            Ventriloquate spell = new Ventriloquate(randomManagerMock.Object, wiznetMock.Object);
            var parameters = BuildParameters("player 'I'm a badass'");
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, parameters.rawParameters, parameters.parameters);

            string result = spell.Guards(abilityActionInput);

            Assert.AreEqual("Just say it.", result);
        }

        [TestMethod]
        public void ValidTarget_Quote()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IWiznet> wiznetMock = new Mock<IWiznet>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityPercentage(It.IsAny<IAbility>())).Returns<IAbility>(x => (100, new AbilityLearned { Name = "Ventriloquate" }));
            Mock<ICharacter> targetMock = new Mock<ICharacter>();
            targetMock.SetupGet(x => x.Name).Returns("target");
            roomMock.SetupGet(x => x.People).Returns(new[] { casterMock.Object, targetMock.Object });
            Ventriloquate spell = new Ventriloquate(randomManagerMock.Object, wiznetMock.Object);
            var parameters = BuildParameters("target 'I'm a badass'");
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, parameters.rawParameters, parameters.parameters);

            string result = spell.Guards(abilityActionInput);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void ValidTarget_NoQuote()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IWiznet> wiznetMock = new Mock<IWiznet>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityPercentage(It.IsAny<IAbility>())).Returns<IAbility>(x => (100, new AbilityLearned { Name = "Ventriloquate" }));
            Mock<ICharacter> targetMock = new Mock<ICharacter>();
            targetMock.SetupGet(x => x.Name).Returns("target");
            roomMock.SetupGet(x => x.People).Returns(new[] { casterMock.Object, targetMock.Object });
            Ventriloquate spell = new Ventriloquate(randomManagerMock.Object, wiznetMock.Object);
            var parameters = BuildParameters("target I'm a badass");
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, parameters.rawParameters, parameters.parameters);

            string result = spell.Guards(abilityActionInput);

            Assert.IsNull(result);
        }
    }
}
