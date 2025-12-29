using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Skills.NonPlayableCharacter;

[CharacterCommand("tail", "Ability", "Skill", "Combat")]
[Skill(SkillName, AbilityEffects.Damage, LearnDifficultyMultiplier = 1, PulseWaitTime = 18)]
public class Tail : FightingSkillBase
{
    private const string SkillName = "Tail";

    public Tail(ILogger<Tail> logger, IRandomManager randomManager, IAbilityManager abilityManager)
        : base(logger, randomManager, abilityManager)
    {
    }

    public override string? Setup(ISkillActionInput skillActionInput)
    {
        var baseSetup = base.Setup(skillActionInput);
        if (baseSetup != null)
            return baseSetup;

        var npcUser = User as INonPlayableCharacter;
        if (Learned == 0
            || npcUser != null && !npcUser.OffensiveFlags.IsSet("Tail"))
            return "You don't have any tail...";

        return null;
    }

    protected override bool Invoke()
    {
        if (RandomManager.Chance(Learned))
        {
            int damage = RandomManager.Range(2*User.Level, 3*User.Level);
            Victim.AbilityDamage(User, damage, SchoolTypes.Bash, "tailsweep", true);
            //DAZE_STATE(victim, 2 * PULSE_VIOLENCE);
            //check_killer(ch,victim);
            return true;
        }
        //check_killer(ch,victim);
        // No need to start a fight because this ability can only used in combat
        return false;
    }
}
