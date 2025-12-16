using Mud.Server.Ability;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;

namespace Mud.Server.Rom24.Weapons;

[Weapon(WeaponName, ["Whip"], LearnDifficultyMultiplier = 5)]
[Help(@"the use of whips, chains, and bullwhips")]
[OneLineHelp(@"the use of whips, chains, and bullwhips")]
public class Whip : IWeaponPassive
{
    private const string WeaponName = "Whip";
}
