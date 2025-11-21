using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Common;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 5)]
[Help(@"the use of knives and daggers, and other stabbing weapons")]
public class Dagger : PassiveBase
{
    private const string PassiveName = "Dagger";

    public Dagger(ILogger<Dagger> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }
}
