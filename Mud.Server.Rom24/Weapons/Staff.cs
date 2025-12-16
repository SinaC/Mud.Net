using Mud.Server.Ability;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;

namespace Mud.Server.Rom24.Weapons;

[Weapon(WeaponName, ["Staff"], LearnDifficultyMultiplier = 5)]
[Help(@"the use of staves")]
[OneLineHelp(@"the use of staves")]
public class Staff : IWeaponPassive
{
    private const string WeaponName = "Staff(weapon)";
}
