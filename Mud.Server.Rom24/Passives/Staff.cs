using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives;

[Weapon(PassiveName, ["Staff"], LearnDifficultyMultiplier = 5)]
[Help(@"the use of staves")]
public class Staff : PassiveBase, IWeaponPassive
{
    private const string PassiveName = "Staff(weapon)";

    public Staff(ILogger<Staff> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }
}
