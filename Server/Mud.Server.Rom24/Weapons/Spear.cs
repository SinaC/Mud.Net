using Mud.Server.Ability;
using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.Ability;

namespace Mud.Server.Rom24.Weapons;

[Weapon(WeaponName, ["Spear"], LearnDifficultyMultiplier = 5)]
[Help(@"this skill covers both spears and staves, but not polearms")]
[OneLineHelp(@"this skill covers both spears and staves, but not polearms")]
public class Spear : IWeaponPassive
{
    private const string WeaponName = "Spear";
}
