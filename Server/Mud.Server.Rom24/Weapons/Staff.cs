using Mud.Server.Ability.Passive.Interfaces;
using Mud.Server.Domain.Attributes;

namespace Mud.Server.Rom24.Weapons;

[Weapon(WeaponName, ["Staff"], LearnDifficultyMultiplier = 5)]
[Help(@"the use of staves")]
[OneLineHelp(@"the use of staves")]
public class Staff : IWeaponPassive
{
    private const string WeaponName = "Staff(weapon)";
}
