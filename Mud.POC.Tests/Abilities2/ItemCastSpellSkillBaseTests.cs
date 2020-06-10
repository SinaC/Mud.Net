using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Container;
using Mud.POC.Abilities2;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Rom24Spells;
using Mud.Server.Common;

namespace Mud.POC.Tests.Abilities2
{
    [TestClass]
    public class ItemCastSpellSkillBaseTests : TestBase
    {
        public const string SkillName = "ItemCastSpellSkillBaseTests_Skill";

        [TestMethod]
        public void CastFireballFromItem()
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
            targetMock.SetupGet(x => x.Name).Returns("target");
            targetMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            roomMock.SetupGet(x => x.People).Returns(new[] { userMock.Object, targetMock.Object});

            var parameters = BuildParameters("target");
            SkillActionInput skillActionInput = new SkillActionInput(new AbilityInfo(skill.GetType()), userMock.Object, parameters.rawParameters, parameters.parameters);
            string result = skill.Setup(skillActionInput);

            skill.Execute();

            Assert.IsNull(result);
            targetMock.Verify(x => x.AbilityDamage(userMock.Object, It.IsAny<int>(), SchoolTypes.Fire, "fireball", It.IsAny<bool>()), Times.Once);
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

            protected override string SetTargets(SkillActionInput skillActionInput)
            {
                return SetupSpell(SpellName, SpellLevel, skillActionInput.RawParameters, skillActionInput.Parameters);
            }
        }
    }
}
