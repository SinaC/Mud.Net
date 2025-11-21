using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Common;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 5)]
[Help(@"the use of staves")]
public class Staff : PassiveBase
{
    private const string PassiveName = "Staff(weapon)";

    public Staff(ILogger<Staff> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }
}
