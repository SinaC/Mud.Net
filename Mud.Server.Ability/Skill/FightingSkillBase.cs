using Microsoft.Extensions.Logging;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Ability.Skill;

public abstract class FightingSkillBase : SkillBase
{
    protected FightingSkillBase(ILogger<FightingSkillBase> logger, IRandomManager randomManager, IAbilityManager abilityManager)
        : base(logger, randomManager, abilityManager)
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
