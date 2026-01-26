using Microsoft.Extensions.Logging;
using Mud.Flags;
using Mud.Random;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.Aura;
using Mud.Server.POC.Affects;

namespace Mud.Server.POC.Skills;

[CharacterCommand("maul", "Ability", "Skill", "Combat")]
[Skill(SkillName, AbilityEffects.Damage), Shapes([Shapes.Bear])]
[Help(
@"Increases the druid's next attack by 128 damage.")]
//https://www.wowhead.com/classic/spell=9881/maul
public class Maul : OffensiveSkillBase
{
    private const string SkillName = "Maul";

    private IAuraManager AuraManager { get; }

    public Maul(ILogger<Maul> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        AuraManager = auraManager;
    }

    protected override bool Invoke()
    {
        var maulAura = User.GetAura(SkillName);
        if (maulAura == null)
        {
            AuraManager.AddAura(User, SkillName, User, User.Level, new AuraFlags("Permanent", "NoSave"), true,
                new NextHitDamageModifierAffect { Modifier = 128 });
            return true;
        }
        return false;
    }
}