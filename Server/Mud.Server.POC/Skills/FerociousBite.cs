using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Random;

namespace Mud.Server.POC.Skills;

[CharacterCommand("ferociousbite", "Ability", "Skill", "Combat")]
[Skill(SkillName, AbilityEffects.Damage), Shapes([Shapes.Cat])]
[Help(
@"Finishing move that causes damage per combo point and converts each extra point of energy into 2.7 additional damage.  Damage is increased by your Attack Power.
  1 point  : 199-259 damage
  2 points: 346-406 damage
  3 points: 493-553 damage
  4 points: 640-700 damage
  5 points: 787-847 damage")]
//https://www.wowhead.com/classic/spell=31018/ferocious-bite
public class FerociousBite : OffensiveSkillBase
{
    private const string SkillName = "Ferocious Bite";

    public FerociousBite(ILogger<FerociousBite> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override bool Invoke()
    {
        var comboCost = ResourceCostsToPay?.SingleOrDefault(x => x.ResourceKind == ResourceKinds.Combo)?.CostAmount ?? 0;
        var comboDamage = comboCost switch
        {
            1 => RandomManager.Range(199,259),
            2 => RandomManager.Range(346, 406),
            3 => RandomManager.Range(493, 553),
            4 => RandomManager.Range(640, 700),
            5 => RandomManager.Range(787, 847),
            _ => 0
        };
        var energyCost = ResourceCostsToPay?.SingleOrDefault(x => x.ResourceKind == ResourceKinds.Energy)?.CostAmount ?? 0;
        var energyDamage = energyCost * 2.7m;
        var damage = (int)(comboDamage + energyDamage);
        Victim.AbilityDamage(User, damage, SchoolTypes.Pierce, "ferocious bite", true);
        //check_killer(ch,victim);
        return true;
    }
}
