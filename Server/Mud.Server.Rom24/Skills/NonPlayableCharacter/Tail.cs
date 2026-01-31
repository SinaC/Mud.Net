using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Common;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Rom24.Skills.NonPlayableCharacter;

[CharacterCommand("tail", "Ability", "Skill", "Combat")]
[Skill(SkillName, AbilityEffects.Damage, LearnDifficultyMultiplier = 1, PulseWaitTime = 18)]
public class Tail : FightingSkillBase
{
    private const string SkillName = "Tail";

    protected override IGuard<ICharacter>[] Guards => [];

    public Tail(ILogger<Tail> logger, IRandomManager randomManager)
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
            Victim.SetDaze(2*Pulse.PulseViolence);
            //check_killer(ch,victim);
            return true;
        }
        //check_killer(ch,victim);
        // No need to start a fight because this ability can only used in combat
        return false;
    }
}
