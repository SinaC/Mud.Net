using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Ability.Skill;
using Mud.Server.Ability.Skill.Interfaces;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Rom24.Skills.NonPlayableCharacter;

[CharacterCommand("bite", "Ability", "Skill", "Combat")]
[Skill(SkillName, AbilityEffects.Damage, LearnDifficultyMultiplier = 1, PulseWaitTime = 12)]
public class Bite : FightingSkillBase
{
    private const string SkillName = "Bite";

    protected override IGuard<ICharacter>[] Guards => [];

    public Bite(ILogger<Bite> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    public override string? Setup(ISkillActionInput skillActionInput)
    {
        var baseSetup = base.Setup(skillActionInput);
        if (baseSetup != null)
            return baseSetup;

        var npcUser = User as INonPlayableCharacter;
        if (Learned == 0
            || npcUser != null && !npcUser.OffensiveFlags.IsSet("Bite"))
            return "Hmmm...";

        return null;
    }

    protected override bool Invoke()
    {
        if (RandomManager.Chance(Learned))
        {
            int damage = RandomManager.Range(1, 2 * User.Level);
            Victim.AbilityDamage(User, damage, SchoolTypes.Pierce, "bite", true);
            //check_killer(ch,victim);
            return true;
        }
        //check_killer(ch,victim);
        // No need to start a fight because this ability can only used in combat
        return false;
    }
}
