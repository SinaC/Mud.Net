using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Ability.Skill;

public abstract class NoTargetSkillBase : SkillBase
{
    protected NoTargetSkillBase(IRandomManager randomManager) 
        : base(randomManager)
    {
    }

    protected override string SetTargets(ISkillActionInput skillActionInput)
        => null!;
}
