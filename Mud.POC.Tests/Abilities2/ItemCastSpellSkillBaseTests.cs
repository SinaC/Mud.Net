using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Container;
using Mud.POC.Abilities2;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Rom24Spells;
using Mud.Server.Common;
using Mud.Server.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.POC.Tests.Abilities2
{
    [TestClass]
    public class ItemCastSpellSkillBaseTests : TestBase
    {
        public const string SkillName = "ItemCastSpellSkillBaseTests_Skill";

        [TestMethod]
        public void Test()
        {
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            ItemCastSpellSkillBaseTestsSkill skill = new ItemCastSpellSkillBaseTestsSkill(randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object, "Fireball", 50);

            abilityManagerMock.Setup(x => x.Search("Fireball", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(typeof(Fireball)));
            //DependencyContainer.Current.Register(typeof(Fireball));
            Fireball fireball = new Fireball(randomManagerMock.Object);
            DependencyContainer.Current.RegisterInstance(fireball);

            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            Mock<ICharacter> targetMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            //userMock.Setup(x => x.GetAbilityLearned(It.IsAny<string>())).Returns<string>(x => (100, new AbilityLearned { Name = SpellName }));
            targetMock.SetupGet(x => x.Name).Returns("target");
            targetMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            roomMock.SetupGet(x => x.People).Returns(new[] { userMock.Object, targetMock.Object});

            var parameters = BuildParameters("target");
            AbilityActionInput abilityActionInput = new AbilityActionInput(new AbilityInfo(skill.GetType()), userMock.Object, parameters.rawParameters, parameters.parameters);
            string result = skill.Setup(abilityActionInput);

            will failed because user doesn't know fireball

            Assert.IsNull(result);
        }

        [Skill(SkillName, AbilityEffects.None)]
        internal class ItemCastSpellSkillBaseTestsSkill : ItemCastSpellSkillBase<IItemScroll>
        {
            protected string SpellName { get; }
            protected int SpellLevel { get; }

            public ItemCastSpellSkillBaseTestsSkill(IRandomManager randomManager, IAbilityManager abilityManager, IItemManager itemManager, string spellName, int spellLevel)
                : base(randomManager, abilityManager, itemManager)
            {
                SpellName = spellName;
                SpellLevel = spellLevel;
            }

            protected override bool Invoke()
            {
                CastSpells();
                return true;
            }

            protected override string SetTargets(AbilityActionInput abilityActionInput)
            {
                return SetSpellTargets(SpellName, SpellLevel, abilityActionInput.RawParameters, abilityActionInput.Parameters);
            }
        }
    }
}
