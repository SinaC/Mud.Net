using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.POC.Abilities2;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Rom24Spells;
using Mud.Server.Random;

namespace Mud.POC.Tests.Abilities2
{
    [TestClass]
    public class ColourSprayTests : TestBase
    {
        [TestMethod]
        public void BlindnessCalledAfterDamageEffectEvenIfBlindnessIsNotKnown()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAuraManager> auraManagerMock = new Mock<IAuraManager>();
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> casterMock = new Mock<ICharacter>();
            Mock<ICharacter> targetMock = new Mock<ICharacter>();

            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            casterMock.SetupGet(x => x.Name).Returns("caster");
            casterMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(x => (100, new AbilityLearned { Name = "Colour Spray" }));
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.CanSee(targetMock.Object)).Returns<IEntity>(_ => true);
            targetMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            targetMock.SetupGet(x => x.Name).Returns("target");
            targetMock.Setup(x => x.AbilityDamage(It.IsAny<ICharacter>(), It.IsAny<int>(), It.IsAny<SchoolTypes>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(DamageResults.Done);
            roomMock.SetupGet(x => x.People).Returns(new[] {casterMock.Object, targetMock.Object});

            var parameters = BuildParameters("target");
            ColourSpray spell = new ColourSpray(randomManagerMock.Object, auraManagerMock.Object);
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, 10, null, parameters.rawParameters, parameters.parameters);
            spell.Setup(abilityActionInput);

            spell.Execute();

            auraManagerMock.Verify(x => x.AddAura(targetMock.Object, "Blindness", casterMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
        }
    }
}
