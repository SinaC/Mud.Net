using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Ability.Skill;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.POC.Skills;

[CharacterCommand("claw", "Ability", "Skill", "Combat")]
[Skill(SkillName, AbilityEffects.Damage)]
[Help(
@"Claw the enemy, causing 115 additional damage.  Awards 1 combo point.")]
//https://www.wowhead.com/classic/spell=9850/claw
public class Claw : OffensiveSkillBase
{
    private const string SkillName = "Claw";

    protected override IGuard<ICharacter>[] Guards => [new RequiresShapesGuard([Shapes.Cat])];

    public Claw(ILogger<Claw> logger, IRandomManager randomManager)
        : base(logger, randomManager)
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
