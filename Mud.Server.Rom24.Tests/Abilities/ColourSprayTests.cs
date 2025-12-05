using Microsoft.Extensions.Logging;
using Mono.Cecil;
using Moq;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Room;
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;
using Mud.Server.Rom24.Spells;

namespace Mud.Server.Rom24.Tests.Abilities
{
    [TestClass]
    public class ColourSprayTests : AbilityTestBase
    {
        [TestMethod]
        public void BlindnessCalledAfterDamageEffectEvenIfBlindnessIsNotKnown()
        {
            Mock<IRandomManager> randomManagerMock = new();
            Mock<IAuraManager> auraManagerMock = new();
            Mock<IEffectManager> effectManagerMock = new();
            Mock<IRoom> roomMock = new();
            Mock<ICharacter> casterMock = new();
            Mock<ICharacter> targetMock = new();

            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            casterMock.SetupGet(x => x.Name).Returns("caster");
            casterMock.Setup(x => x.GetAbilityLearnedInfo(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.CanSee(targetMock.Object)).Returns<IEntity>(_ => true);
            targetMock.SetupGet(x => x.CharacterFlags).Returns(_characterFlagFactory.CreateInstance());
            targetMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            targetMock.SetupGet(x => x.Name).Returns("target");
            targetMock.SetupGet(x => x.Keywords).Returns("target".Yield());
            casterMock.SetupGet(x => x.CharacterFlags).Returns(_characterFlagFactory.CreateInstance());
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            targetMock.Setup(x => x.AbilityDamage(It.IsAny<ICharacter>(), It.IsAny<int>(), It.IsAny<SchoolTypes>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(DamageResults.Done);
            roomMock.SetupGet(x => x.People).Returns([casterMock.Object, targetMock.Object]);
            effectManagerMock.Setup(x => x.CreateInstance<ICharacter>(It.IsAny<string>()))
                .Returns(() => new BlindnessEffect(_characterFlagFactory, auraManagerMock.Object));

            var parameters = BuildParameters("target");
            ColourSpray spell = new (new Mock<ILogger<ColourSpray>>().Object, randomManagerMock.Object, effectManagerMock.Object);
            SpellActionInput abilityActionInput = new (new AbilityInfo( spell.GetType()), casterMock.Object, 10, null, parameters);
            var setupResult = spell.Setup(abilityActionInput);

            spell.Execute();

            Assert.IsNull(setupResult);
            effectManagerMock.Verify(x => x.CreateInstance<ICharacter>("Blindness"), Times.Once);
            auraManagerMock.Verify(x => x.AddAura(targetMock.Object, "Blindness", casterMock.Object, It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<AuraFlags>(), It.IsAny<bool>(), It.IsAny<IAffect[]>()), Times.Once);
        }
    }
}
