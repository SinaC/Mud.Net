using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Flags;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Room;
using Mud.Server.Random;
using Mud.Server.Rom24.Spells;

namespace Mud.Server.Tests.Abilities
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
            casterMock.Setup(x => x.GetAbilityLearnedInfo(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.CanSee(targetMock.Object)).Returns<IEntity>(_ => true);
            targetMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            targetMock.SetupGet(x => x.Name).Returns("target");
            targetMock.SetupGet(x => x.Keywords).Returns("target".Yield());
            targetMock.SetupGet(x => x.CharacterFlags).Returns(new CharacterFlags());
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            targetMock.Setup(x => x.AbilityDamage(It.IsAny<ICharacter>(), It.IsAny<int>(), It.IsAny<SchoolTypes>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(DamageResults.Done);
            roomMock.SetupGet(x => x.People).Returns(new[] {casterMock.Object, targetMock.Object});

            var parameters = BuildParameters("target");
            ColourSpray spell = new ColourSpray(randomManagerMock.Object, auraManagerMock.Object);
            SpellActionInput abilityActionInput = new SpellActionInput(new AbilityInfo(spell.GetType()), casterMock.Object, 10, null, parameters);
            string setupResult = spell.Setup(abilityActionInput);

            spell.Execute();

            Assert.IsNull(setupResult);
            auraManagerMock.Verify(x => x.AddAura(targetMock.Object, "Blindness", casterMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
        }
    }
}
