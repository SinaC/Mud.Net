using Mud.Server.Ability.Passive.Interfaces;
using Mud.Server.Domain.Attributes;

namespace Mud.Server.Rom24.Weapons;

[Weapon(WeaponName, ["Dagger"], LearnDifficultyMultiplier = 5)]
[Help(@"the use of knives and daggers, and other stabbing weapons")]
[OneLineHelp(@"the use of knives and daggers, and other stabbing weapons")]
public class Dagger : IWeaponPassive
{
    private const string WeaponName = "Dagger";
}
