using Microsoft.Extensions.Logging;
using Mud.Server.Interfaces.Ability;
using Mud.Random;

namespace Mud.Server.Ability.Skill;

public abstract class NoTargetSkillBase : SkillBase
{
    protected NoTargetSkillBase(ILogger<NoTargetSkillBase> logger, IRandomManager randomManager) 
        : base(logger, randomManager)
    {
    }

    protected override string SetTargets(ISkillActionInput skillActionInput)
        => null!;
}
