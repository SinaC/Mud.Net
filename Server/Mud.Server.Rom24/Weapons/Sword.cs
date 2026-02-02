using Mud.Server.Ability.Passive.Interfaces;
using Mud.Server.Domain.Attributes;

namespace Mud.Server.Rom24.Weapons;

[Weapon(WeaponName, ["Sword"], LearnDifficultyMultiplier = 5)]
[Help(@"the warrior's standby, from rapier to claymore")]
[OneLineHelp(@"the warrior's standby, from rapier to claymore")]
public class Sword : IWeaponPassive
{
    private const string WeaponName = "Sword";
}
