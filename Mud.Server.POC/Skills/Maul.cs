using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Random;

namespace Mud.Server.POC.Skills;

[CharacterCommand("maul", "Ability", "Skill", "Combat")]
[Skill(SkillName, AbilityEffects.Damage)]
[AbilityShape(Shapes.Bear)]
[Help(
@"Increases the druid's next attack by 128 damage.")]
//https://www.wowhead.com/classic/spell=9881/maul
public class Maul : OffensiveSkillBase
{
    private const string SkillName = "Maul";

    public Maul(ILogger<Maul> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override bool Invoke()
    {
        return false;
    }
}