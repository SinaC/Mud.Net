using Microsoft.Extensions.Logging;
using Moq;
using Mud.Common;
using Mud.Domain;
using Mud.Flags;
using Mud.Random;
using Mud.Server.Ability;
using Mud.Server.Ability.Interfaces;
using Mud.Server.Ability.Skill;
using Mud.Server.Ability.Skill.Interfaces;
using Mud.Server.Ability.Spell.Interfaces;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Tests.Mocking;

namespace Mud.Server.Tests.Abilities;

[TestClass]
public class ItemCastSpellSkillBaseTests : AbilityTestBase
{
    public const string SkillName = "ItemCastSpellSkillBaseTests_Skill";

    [TestMethod]
    public void CastAcidBlastFromItem()
    {
        Mock<ILogger> logger = new();
        Mock<IRandomManager> randomManagerMock = new();
        Mock<IAbilityManager> abilityManagerMock = new();
        Mock<IItemManager> itemManagerMock = new();
        ItemCastSpellSkillBaseTestsSkill skill = new(new Mock<ILogger<ItemCastSpellSkillBaseTestsSkill>>().Object, randomManagerMock.Object, abilityManagerMock.Object, itemManagerMock.Object, "Acid Blast", 50);

        abilityManagerMock.Setup(x => x.Get("Acid Blast", AbilityTypes.Spell)).Returns<string, AbilityTypes>((x, y) => new AbilityDefinition(typeof(Rom24AcidBlast)));
        abilityManagerMock.Setup(x => x.CreateInstance<ISpell>(It.IsAny<string>())).Returns<string>(x => new Rom24AcidBlast(new Mock<ILogger<Rom24AcidBlast>>().Object, randomManagerMock.Object));

        Mock<IRoom> roomMock = new();
        Mock<ICharacter> userMock = new();
        Mock<ICharacter> targetMock = new();
        userMock.SetupGet(x => x.Name).Returns("user");
        userMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        userMock.SetupGet(x => x.ImmortalMode).Returns(new ImmortalModes());
        userMock.Setup(x => x.CanSee(It.IsAny<ICharacter>())).Returns(true);
        targetMock.SetupGet(x => x.Name).Returns("target");
        targetMock.SetupGet(x => x.Keywords).Returns("target".Yield());
        targetMock.SetupGet(x => x.Room).Returns(roomMock.Object);
        roomMock.SetupGet(x => x.People).Returns([userMock.Object, targetMock.Object]);

        var actionInput = BuildActionInput<ItemCastSpellSkillBaseTestsSkill>(userMock.Object, "whatever target");
        SkillActionInput skillActionInput = new(actionInput, new AbilityDefinition(skill.GetType()), userMock.Object);
        var result = skill.Setup(skillActionInput);

        skill.Execute();

        Assert.IsNull(result);
        targetMock.Verify(x => x.AbilityDamage(userMock.Object, It.IsAny<int>(), SchoolTypes.Acid, "acid blast", It.IsAny<bool>()), Times.Once);
    }

    [CharacterCommand("ItemCastSpellSkillBaseTestsSkill")]
    [Skill(SkillName, AbilityEffects.None)]
    public class ItemCastSpellSkillBaseTestsSkill : ItemCastSpellSkillBase<IItemScroll>
    {
        protected override IGuard<ICharacter>[] Guards => [];

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

        protected override string? SetTargets(ISkillActionInput skillActionInput)
        {
            return SetupSpell(SpellName, SpellLevel, skillActionInput.Parameters);
        }
    }
}
