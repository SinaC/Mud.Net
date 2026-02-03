using Microsoft.Extensions.Logging;
using Mud.Random;
using Mud.Server.Ability.Skill.Interfaces;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Ability.Skill;

public abstract class FightingSkillBase : SkillBase
{
    protected FightingSkillBase(ILogger<FightingSkillBase> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected ICharacter Victim { get; private set; } = default!;

    protected override string? SetTargets(ISkillActionInput skillActionInput)
    {
        Victim = User.Fighting!;
        if (Victim == null)
            return "You aren't fighting anyone.";
        // Victim found
        return null;
    }
}
