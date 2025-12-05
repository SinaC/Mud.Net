using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives;

[Weapon(PassiveName, ["Mace"], LearnDifficultyMultiplier = 5)]
[Help(@"this skill includes clubs and hammers as well as maces")]
public class Mace : PassiveBase, IWeaponPassive
{
    private const string PassiveName = "Mace";

    public Mace(ILogger<Mace> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }
}
