using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.POC.Skills;

[CharacterCommand("claw", "Ability", "Skill", "Combat")]
[Skill(SkillName, AbilityEffects.Damage), Shapes([Shapes.Cat])]
[Help(
@"Claw the enemy, causing 115 additional damage.  Awards 1 combo point.")]
//https://www.wowhead.com/classic/spell=9850/claw
public class Claw : OffensiveSkillBase
{
    private const string SkillName = "Claw";

    public Claw(ILogger<Claw> logger, IRandomManager randomManager, IAbilityManager abilityManager)
        : base(logger, randomManager, abilityManager)
    {
    }

    protected override bool Invoke()
    {
        var damage = RandomManager.Range(1, User.Level); // same damage as kick
        Victim.AbilityDamage(User, damage, SchoolTypes.Pierce, "claw", true);
        //check_killer(ch,victim);
        User.UpdateResource(ResourceKinds.Combo, 1);
        return true;
    }
}
