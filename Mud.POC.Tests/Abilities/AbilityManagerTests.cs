using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.POC.Abilities;
using Mud.Server.Random;
using System.Linq;
using System.Reflection;

namespace Mud.POC.Tests.Abilities
{
    [TestClass]
    public class AbilityManagerTests
    {
        [TestMethod]
        public void AbilityManager_Ctor_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);

            Assert.AreEqual(typeof(Spells).GetMethods(BindingFlags.Static | BindingFlags.Public).Count(m => !m.IsSpecialName) + typeof(Skills).GetMethods(BindingFlags.Static | BindingFlags.Public).Count(m => !m.IsSpecialName) + Passives.Abilities.Count, abilityManager.Abilities.Count());
            Assert.AreEqual(typeof(Spells).GetMethods(BindingFlags.Static | BindingFlags.Public).Count(m => !m.IsSpecialName), abilityManager.Abilities.Count(x => x.Kind == AbilityKinds.Spell));
            Assert.AreEqual(typeof(Skills).GetMethods(BindingFlags.Static | BindingFlags.Public).Count(m => !m.IsSpecialName), abilityManager.Abilities.Count(x => x.Kind == AbilityKinds.Skill));
            Assert.AreEqual(Passives.Abilities.Count, abilityManager.Abilities.Count(x => x.Kind == AbilityKinds.Passive));
            Assert.AreEqual(abilityManager.Passives.Count(), abilityManager.Abilities.Count(x => x.Kind == AbilityKinds.Passive));
            Assert.AreEqual(abilityManager.Spells.Count(), abilityManager.Abilities.Count(x => x.Kind == AbilityKinds.Spell));
            Assert.AreEqual(abilityManager.Skills.Count(), abilityManager.Abilities.Count(x => x.Kind == AbilityKinds.Skill));
        }

        [TestMethod]
        public void AbilityManager_Indexer_ExistingName_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            IAbility thirdAbility = abilityManager.Abilities.Skip(2).First();

            IAbility ability = abilityManager[thirdAbility.Name];

            Assert.IsNotNull(ability);
            Assert.AreSame(thirdAbility, ability);
        }

        [TestMethod]
        public void AbilityManager_Indexer_NonExistingName_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);

            IAbility ability = abilityManager["pouet"];

            Assert.IsNull(ability);
        }
    }
}
