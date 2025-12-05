using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives;

[Weapon(PassiveName, ["Axe"], LearnDifficultyMultiplier = 5)]
[Help(@"the use of axes, ranging from hand to great (but not halberds)")]
public class Axe : PassiveBase, IWeaponPassive
{
    private const string PassiveName = "Axe";

    public Axe(ILogger<Axe> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }
}
