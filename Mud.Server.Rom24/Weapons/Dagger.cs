using Mud.Server.Ability;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;

namespace Mud.Server.Rom24.Weapons;

[Weapon(WeaponName, ["Dagger"], LearnDifficultyMultiplier = 5)]
[Help(@"the use of knives and daggers, and other stabbing weapons")]
[OneLineHelp(@"the use of knives and daggers, and other stabbing weapons")]
public class Dagger : IWeaponPassive
{
    private const string WeaponName = "Dagger";
}
