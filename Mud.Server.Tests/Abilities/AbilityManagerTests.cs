using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Server.Common;
using System.Linq;
using Mud.Server.Abilities;

namespace Mud.Server.Tests.Abilities
{
    [TestClass]
    public class AbilityManagerTests
    {
        [TestMethod]
        public void AbilityManager_Ctor_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object, null, null, null, null);

            Assert.AreEqual(abilityManager.Passives.Count(), abilityManager.Abilities.Count(x => x.Kind == Domain.AbilityKinds.Passive));
            Assert.AreEqual(abilityManager.Spells.Count(), abilityManager.Abilities.Count(x => x.Kind == Domain.AbilityKinds.Spell));
            Assert.AreEqual(abilityManager.Skills.Count(), abilityManager.Abilities.Count(x => x.Kind == Domain.AbilityKinds.Skill));
        }

        [TestMethod]
        public void AbilityManager_Indexer_ExistingName_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object, null, null, null, null);
            IAbility thirdAbility = abilityManager.Abilities.Skip(2).First();

            IAbility ability = abilityManager[thirdAbility.Name];

            Assert.IsNotNull(ability);
            Assert.AreSame(thirdAbility, ability);
        }

        [TestMethod]
        public void AbilityManager_Indexer_NonExistingName_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object, null, null, null, null);

            IAbility ability = abilityManager["pouet"];

            Assert.IsNull(ability);
        }
    }
}
