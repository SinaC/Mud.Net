using Microsoft.Extensions.Logging;
using Moq;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Flags;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Room;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Tests.Abilities
{
    [TestClass]
    public class OffensiveSpellBaseSpells : AbilityTestBase
    {
        public const string SpellName = "OffensiveSpellBaseSpells_Spell";

        [TestMethod]
        public void Setup_NoTarget()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityLearnedInfo(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            OffensiveSpellBaseSpellsSpell spell = new (new Mock<ILogger<OffensiveSpellBaseSpellsSpell>>().Object, randomManagerMock.Object);

            var parameters = BuildParameters("");
            SpellActionInput abilityActionInput = new (new AbilityInfo(new Mock<ILogger>().Object, spell.GetType()), casterMock.Object, 10, null, parameters);

            var result = spell.Setup(abilityActionInput);

            Assert.AreEqual("Cast the spell on whom?", result);
        }

        [TestMethod]
        public void Setup_NoTarget_Figthing()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityLearnedInfo(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            Mock<ICharacter> victimMock = new Mock<ICharacter>();
            victimMock.SetupGet(x => x.Name).Returns("target");
            victimMock.SetupGet(x => x.Keywords).Returns("target".Yield());
            victimMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            roomMock.SetupGet(x => x.People).Returns(new[] { casterMock.Object, victimMock.Object });
            casterMock.SetupGet(x => x.Fighting).Returns(victimMock.Object);
            OffensiveSpellBaseSpellsSpell spell = new (new Mock<ILogger<OffensiveSpellBaseSpellsSpell>>().Object, randomManagerMock.Object);

            var parameters = BuildParameters("");
            SpellActionInput abilityActionInput = new (new AbilityInfo(new Mock<ILogger>().Object, spell.GetType()), casterMock.Object, 10, null, parameters);

            var result = spell.Setup(abilityActionInput);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Setup_TargetNotFound()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityLearnedInfo(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns(ResourceKinds.Mana.Yield());
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            OffensiveSpellBaseSpellsSpell spell = new (new Mock<ILogger<OffensiveSpellBaseSpellsSpell>>().Object, randomManagerMock.Object);

            var parameters = BuildParameters("target");
            SpellActionInput abilityActionInput = new (new AbilityInfo(new Mock<ILogger>().Object, spell.GetType()), casterMock.Object, 10, null, parameters);

            var result = spell.Setup(abilityActionInput);

            Assert.AreEqual("They aren't here.", result);
        }

        [TestMethod]
        public void Setup_TargetNotFoundInRoom()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityLearnedInfo(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns([ResourceKinds.Mana]);
            casterMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns(true);
            Mock<ICharacter> victimMock = new Mock<ICharacter>();
            victimMock.SetupGet(x => x.Name).Returns("target");
            victimMock.SetupGet(x => x.Keywords).Returns("target".Yield());
            roomMock.SetupGet(x => x.People).Returns(casterMock.Object.Yield());
            OffensiveSpellBaseSpellsSpell spell = new (new Mock<ILogger<OffensiveSpellBaseSpellsSpell>>().Object, randomManagerMock.Object);

            var parameters = BuildParameters("target");
            SpellActionInput abilityActionInput = new (new AbilityInfo(new Mock<ILogger>().Object, spell.GetType()), casterMock.Object, 10, null, parameters);

            var result = spell.Setup(abilityActionInput);

            Assert.AreEqual("They aren't here.", result);
        }

        [TestMethod]
        public void Setup_SafeTarget()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityLearnedInfo(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns([ResourceKinds.Mana]);
            casterMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns(true);
            Mock<ICharacter> victimMock = new Mock<ICharacter>();
            victimMock.SetupGet(x => x.Name).Returns("target");
            victimMock.SetupGet(x => x.Keywords).Returns("target".Yield());
            victimMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            victimMock.Setup(x => x.IsSafe(casterMock.Object)).Returns<ICharacter>(_ => "Not on that victim!!!!");
            roomMock.SetupGet(x => x.People).Returns([casterMock.Object, victimMock.Object]);
            OffensiveSpellBaseSpellsSpell spell = new (new Mock<ILogger<OffensiveSpellBaseSpellsSpell>>().Object, randomManagerMock.Object);

            var parameters = BuildParameters("target");
            SpellActionInput abilityActionInput = new (new AbilityInfo(new Mock<ILogger>().Object, spell.GetType()), casterMock.Object, 10, null, parameters);

            var result = spell.Setup(abilityActionInput);

            Assert.AreEqual("Not on that victim!!!!", result);
        }

        [TestMethod]
        public void Setup_NotOnMaster()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IPlayableCharacter> victimMock = new Mock<IPlayableCharacter>();
            victimMock.SetupGet(x => x.Name).Returns("target");
            victimMock.SetupGet(x => x.Keywords).Returns("target".Yield());
            victimMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            Mock<INonPlayableCharacter> casterMock = new Mock<INonPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityLearnedInfo(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x.CharacterFlags).Returns(_characterFlagFactory.CreateInstance("Charm"));
            casterMock.SetupGet(x => x.Master).Returns(victimMock.Object);
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns([ResourceKinds.Mana]);
            casterMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns(true);
            roomMock.SetupGet(x => x.People).Returns([casterMock.Object, victimMock.Object]);
            OffensiveSpellBaseSpellsSpell spell = new (new Mock<ILogger<OffensiveSpellBaseSpellsSpell>>().Object, randomManagerMock.Object);

            var parameters = BuildParameters("target");
            SpellActionInput abilityActionInput = new (new AbilityInfo(new Mock<ILogger>().Object, spell.GetType()), casterMock.Object, 10, null, parameters);

            var result = spell.Setup(abilityActionInput);

            Assert.AreEqual("You can't do that on your own follower.", result);
        }

        [TestMethod]
        public void Setup_TargetSpecifiedAndFound()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Chance(It.IsAny<int>())).Returns<int>(_ => true);
            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<IPlayableCharacter> casterMock = new Mock<IPlayableCharacter>();
            casterMock.SetupGet(x => x.Name).Returns("player");
            casterMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            casterMock.Setup(x => x.GetAbilityLearnedInfo(It.IsAny<string>())).Returns<string>(abilityName => (100, BuildAbilityLearned(abilityName)));
            casterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(100);
            casterMock.SetupGet(x => x.CurrentResourceKinds).Returns([ResourceKinds.Mana]);
            casterMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns(true);
            Mock<ICharacter> victimMock = new Mock<ICharacter>();
            victimMock.SetupGet(x => x.Name).Returns("target");
            victimMock.SetupGet(x => x.Keywords).Returns("target".Yield());
            victimMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            roomMock.SetupGet(x => x.People).Returns([casterMock.Object, victimMock.Object]);
            OffensiveSpellBaseSpellsSpell spell = new (new Mock<ILogger<OffensiveSpellBaseSpellsSpell>>().Object, randomManagerMock.Object);

            var parameters = BuildParameters("target");
            SpellActionInput abilityActionInput = new (new AbilityInfo(new Mock<ILogger>().Object, spell.GetType()), casterMock.Object, 10, null, parameters);

            var result = spell.Setup(abilityActionInput);

            Assert.IsNull(result);
        }

        // Spell without specific Setup nor invoke
        [Spell(SpellName, AbilityEffects.Damage)]
        public class OffensiveSpellBaseSpellsSpell : OffensiveSpellBase
        {
            public OffensiveSpellBaseSpellsSpell(ILogger<OffensiveSpellBaseSpellsSpell> logger, IRandomManager randomManager)
                : base(logger, randomManager)
            {
            }

            protected override void Invoke()
            {
            }
        }
    }
}
