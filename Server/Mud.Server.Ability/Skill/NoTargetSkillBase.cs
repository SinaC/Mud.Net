using Microsoft.Extensions.Logging;
using Mud.Random;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Ability.Skill;

public abstract class NoTargetSkillBase : SkillBase
{
    protected NoTargetSkillBase(ILogger<NoTargetSkillBase> logger, IRandomManager randomManager) 
        : base(logger, randomManager)
    {
    }

    protected override IGuard<ICharacter>[] Guards => [];

    protected override string SetTargets(ISkillActionInput skillActionInput)
        => null!;
}
