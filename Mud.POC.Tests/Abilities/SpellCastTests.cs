using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.POC.Abilities;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.POC.Tests.Abilities
{
    [TestClass]
    public class SpellCastTests : AbilityTestBase
    {
        [TestMethod]
        public void NoParam_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();

            CastResults result = abilityManager.Cast(characterMock.Object, string.Empty);

            Assert.AreEqual(CastResults.MissingParameter, result);
        }

        [TestMethod]
        public void UnknownSpell_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("pouet");
            CastResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CastResults.InvalidParameter, result);
        }

        [TestMethod]
        public void SpellNotKnown_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'Mass invis'");
            CastResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CastResults.InvalidParameter, result);
        }

        [TestMethod]
        public void SpellNotYetLearned_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Mass invis"], Level = 1, ResourceKind = null, Learned = 0 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'Mass invis'");
            CastResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CastResults.InvalidParameter, result);
        }

        [TestMethod]
        public void TooLowLevel_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(5);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Mass invis"], Learned = 1, Level = 20 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'Mass invis'");
            CastResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CastResults.InvalidParameter, result);
        }

        [TestMethod]
        public void Passive_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Whip"], Learned = 1, Level = 20 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("whip");
            CastResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CastResults.InvalidParameter, result);
        }

        [TestMethod]
        public void Skill_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Disarm"], Learned = 1, Level = 20 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("Disarm");
            CastResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CastResults.InvalidParameter, result);
        }

        [TestMethod]
        public void Failed_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(false); // always fails
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Mass invis"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'Mass invis'");
            CastResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CastResults.Failed, result);
        }

        [TestMethod]
        public void NotEnoughResource_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Mass invis"], Level = 1, ResourceKind = ResourceKinds.Mana, CostAmount = 10, CostAmountOperator = CostAmountOperators.Fixed, Learned = 1 } });
            characterMock.SetupGet(x => x.CurrentResourceKinds).Returns(new[] {ResourceKinds.Mana});
            characterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(0); // not enough mana

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'Mass invis'");
            CastResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CastResults.NotEnoughResource, result);
        }

        [TestMethod]
        public void ResourceNotCurrentlyAvailable_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Mass invis"], Level = 1, ResourceKind = ResourceKinds.Mana, CostAmount = 10, CostAmountOperator = CostAmountOperators.Fixed, Learned = 1 } });
            characterMock.SetupGet(x => x.CurrentResourceKinds).Returns(new[] { ResourceKinds.Psy });
            characterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(1000);

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'Mass invis'");
            CastResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CastResults.CantUseRequiredResource, result);
        }

        [TestMethod]
        public void FailedHalfResource_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(false); // always failed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Mass invis"], Level = 1, ResourceKind = ResourceKinds.Mana, CostAmount = 10, CostAmountOperator = CostAmountOperators.Fixed, Learned = 1 } });
            characterMock.SetupGet(x => x.CurrentResourceKinds).Returns(new[] { ResourceKinds.Mana });
            int mana = 1000;
            characterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(mana);
            characterMock.Setup(x => x.UpdateResource(It.IsAny<ResourceKinds>(), It.IsAny<int>())).Callback<ResourceKinds, int>((kind, cost) => mana += cost);

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'Mass invis'");
            CastResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CastResults.Failed, result);
            Assert.AreEqual(995, mana); // teleport costs 10 but spell failed -> 5 mana
        }

        [TestMethod]
        public void NoCost_Success_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Mass invis"], Level = 1, ResourceKind = null, CostAmount = 0, CostAmountOperator = CostAmountOperators.Fixed, Learned = 1 } });
            characterMock.SetupGet(x => x.CurrentResourceKinds).Returns(new[] { ResourceKinds.Mana });
            int mana = 1000;
            characterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(mana);
            characterMock.Setup(x => x.UpdateResource(It.IsAny<ResourceKinds>(), It.IsAny<int>())).Callback<ResourceKinds, int>((kind, cost) => mana += cost);

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'Mass invis'");
            CastResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CastResults.Ok, result);
            Assert.AreEqual(1000, mana); // teleport costs nothing
        }

        [TestMethod]
        public void FixedResourceCost_Success_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Mass invis"], Level = 1, ResourceKind = ResourceKinds.Mana, CostAmount = 10, CostAmountOperator = CostAmountOperators.Fixed, Learned = 1 } });
            characterMock.SetupGet(x => x.CurrentResourceKinds).Returns(new[] { ResourceKinds.Mana });
            int mana = 1000;
            characterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(mana);
            characterMock.Setup(x => x.UpdateResource(It.IsAny<ResourceKinds>(), It.IsAny<int>())).Callback<ResourceKinds, int>((kind, cost) => mana += cost);

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'Mass invis'");
            CastResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CastResults.Ok, result);
            Assert.AreEqual(990, mana); // teleport costs 10
        }

        [TestMethod]
        public void PercentageResourceCost_Success_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Mass invis"], Level = 1, ResourceKind = ResourceKinds.Mana, CostAmount = 10, CostAmountOperator = CostAmountOperators.Percentage, Learned = 1 } });
            characterMock.SetupGet(x => x.CurrentResourceKinds).Returns(new[] { ResourceKinds.Mana });
            int maxMana = 1500;
            int mana = 1000;
            characterMock.SetupGet(x => x[It.IsAny<ResourceKinds>()]).Returns(mana);
            characterMock.Setup(x => x.GetMaxResource(It.IsAny<ResourceKinds>())).Returns<int>(kind => maxMana);
            characterMock.Setup(x => x.UpdateResource(It.IsAny<ResourceKinds>(), It.IsAny<int>())).Callback<ResourceKinds, int>((kind, cost) => mana += cost);

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'Mass invis'");
            CastResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CastResults.Ok, result);
            Assert.AreEqual(850, mana); // teleport costs 10% of 1500 -> -150
        }
    }
}
