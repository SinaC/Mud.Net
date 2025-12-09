using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Weapons;

[Weapon(PassiveName, ["Polearm"], LearnDifficultyMultiplier = 5)]
[Help(@"the use of pole weapons (except spears), including halberds")]
[OneLineHelp(@"the use of pole weapons (except spears), including halberds")]
public class Polearm : PassiveBase, IWeaponPassive
{
    private const string PassiveName = "Polearm";

    public Polearm(ILogger<Polearm> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }
}
