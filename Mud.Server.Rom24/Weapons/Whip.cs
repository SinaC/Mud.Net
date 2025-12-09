using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Weapons;

[Weapon(PassiveName, ["Whip"], LearnDifficultyMultiplier = 5)]
[Help(@"the use of whips, chains, and bullwhips")]
[OneLineHelp(@"the use of whips, chains, and bullwhips")]
public class Whip : PassiveBase, IWeaponPassive
{
    private const string PassiveName = "Whip";

    public Whip(ILogger<Whip> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }
}
