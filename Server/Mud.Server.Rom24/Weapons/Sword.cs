using Mud.Server.Ability;
using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.Ability;

namespace Mud.Server.Rom24.Weapons;

[Weapon(WeaponName, ["Sword"], LearnDifficultyMultiplier = 5)]
[Help(@"the warrior's standby, from rapier to claymore")]
[OneLineHelp(@"the warrior's standby, from rapier to claymore")]
public class Sword : IWeaponPassive
{
    private const string WeaponName = "Sword";
}
