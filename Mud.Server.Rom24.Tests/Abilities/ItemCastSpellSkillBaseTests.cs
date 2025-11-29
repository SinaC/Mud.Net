using Microsoft.Extensions.Logging;
using Moq;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Random;
using Mud.Server.Rom24.Spells;

namespace Mud.Server.Rom24.Tests.Abilities
{
    [TestClass]
    public class ItemCastSpellSkillBaseTests : AbilityTestBase
    {
        public const string SkillName = "ItemCastSpellSkillBaseTests_Skill";

        [TestMethod]
        public void CastFireballFromItem()
        {
            Mock<ILogger> logger = new Mock<ILogger>();
            Mock<IRandomManager> randomManagerMock = new Mock<IRandomManager>();
            Mock<IAbilityManager> abilityManagerMock = new Mock<IAbilityManager>();
            Mock<IItemManager> itemManagerMock = new Mock<IItemManager>();
            ItemCastSpellSkillBaseTestsSkill skill = new ItemCastSpellSkillBaseTestsSkill(new Mock<ILogger<ItemCastSpellSkillBaseTestsSkill>>().Object, randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object, "Fireball", 50);

            abilityManagerMock.Setup(x => x.Search("Fireball", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityInfo(logger.Object, typeof(Fireball)));
            abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Fireball(new Mock<ILogger<Fireball>>().Object, randomManagerMock.Object));

            Mock<IRoom> roomMock = new Mock<IRoom>();
            Mock<ICharacter> userMock = new Mock<ICharacter>();
            Mock<ICharacter> targetMock = new Mock<ICharacter>();
            userMock.SetupGet(x => x.Name).Returns("user");
            userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns(true);
            targetMock.SetupGet(x => x.Name).Returns("target");
            targetMock.SetupGet(x => x.Keywords).Returns("target".Yield());
            targetMock.SetupGet(x => x.Room).Returns(roomMock.Object);
            roomMock.SetupGet(x => x.People).Returns([userMock.Object, targetMock.Object]);

            var actionInput = BuildActionInput<ItemCastSpellSkillBaseTestsSkill>(userMock.Object, "whatever target");
            SkillActionInput skillActionInput = new SkillActionInput(actionInput, new AbilityInfo(logger.Object, skill.GetType()), userMock.Object);
            var result = skill.Setup(skillActionInput);

            skill.Execute();

            Assert.IsNull(result);
            targetMock.Verify(x => x.AbilityDamage(userMock.Object, It.IsAny<int>(), SchoolTypes.Fire, "fireball", It.IsAny<bool>()), Times.Once);
        }

        [Command("ItemCastSpellSkillBaseTestsSkill")]
        [Skill(SkillName, AbilityEffects.None)]
        public class ItemCastSpellSkillBaseTestsSkill : ItemCastSpellSkillBase<IItemScroll>
        {
            protected string SpellName { get; }
            protected int SpellLevel { get; }

            protected override string ActionWord => "Woot";

            public ItemCastSpellSkillBaseTestsSkill(ILogger<ItemCastSpellSkillBaseTestsSkill> logger, IRandomManager randomManager, IAbilityManager abilityManager, IItemManager itemManager, string spellName, int spellLevel)
                : base(logger, randomManager, abilityManager, itemManager)
            {
                SpellName = spellName;
                SpellLevel = spellLevel;
            }

            protected override bool Invoke()
            {
                CastSpells();
                return true;
            }

            protected override string SetTargets(ISkillActionInput skillActionInput)
            {
                return SetupSpell(SpellName, SpellLevel, skillActionInput.Parameters);
            }
        }
    }
}
