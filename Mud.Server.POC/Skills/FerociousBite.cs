using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.POC.Skills;

[CharacterCommand("ferociousbite", "Ability", "Skill", "Combat")]
[Skill(SkillName, AbilityEffects.Damage)]
[AbilityShape(Shapes.Cat)]
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

    public FerociousBite(ILogger<OffensiveSkillBase> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override bool Invoke()
    {
        if (RandomManager.Chance(Learned))
        {
            var comboDamage = Cost switch
            {
                1 => RandomManager.Range(199,259),
                2 => RandomManager.Range(346, 406),
                3 => RandomManager.Range(493, 553),
                4 => RandomManager.Range(640, 700),
                5 => RandomManager.Range(787, 847),
                _ => 0
            };
            var energyDamage = (int)(User[ResourceKinds.Energy] * 2.7);
            var damage = comboDamage + energyDamage;
            Victim.AbilityDamage(User, damage, SchoolTypes.Pierce, "ferocious bite", true);
            //check_killer(ch,victim);
            // use remaining energy
            User.SetResource(ResourceKinds.Energy, 0);
            return true;
        }
        Victim.AbilityDamage(User, 0, SchoolTypes.Pierce, "ferocious bite", true); // start a fight if needed
        return false;
    }
}
