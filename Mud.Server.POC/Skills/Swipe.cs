using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Extensions;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.POC.Skills;

[CharacterCommand("swipe", "Ability", "Skill", "Combat")]
[Skill(SkillName, AbilityEffects.Damage)]
[AbilityShape(Shapes.Bear)]
[Help(
@"Swipe 3 nearby enemies, inflicting 128 damage.")]
//https://www.wowhead.com/classic/spell=9881/maul
public class Swipe : FightingSkillBase
{
    private const string SkillName = "Swipe";

    public Swipe(ILogger<Swipe> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override bool Invoke()
    {
        var victims = User.Room.People.OfType<INonPlayableCharacter>().Where(x => x != User && !x.IsSafeSpell(User, true)).Shuffle(RandomManager).Take(3).ToArray();
        if (victims.Length == 0)
            return false; // should never happen because this skill can only used in combat

        foreach (var victim in victims)
        {
            var damage = 128;
            victim.AbilityDamage(User, damage, SchoolTypes.Slash, "swipe", true);
            if (User.Fighting == null)
                break;
        }

        return true;
    }
}