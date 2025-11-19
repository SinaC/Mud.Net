using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 4)]
public class Haggle : PassiveBase
{
    private const string PassiveName = "Haggle";

    public Haggle(ILogger<Haggle> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }
}
