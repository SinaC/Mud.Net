using Mud.Server.Ability.Passive.Interfaces;
using Mud.Server.Domain.Attributes;

namespace Mud.Server.Rom24.Weapons;

[Weapon(WeaponName, ["Axe"], LearnDifficultyMultiplier = 5)]
[Help(@"the use of axes, ranging from hand to great (but not halberds)")]
[OneLineHelp(@"the use of axes, ranging from hand to great (but not halberds)")]
public class Axe : IWeaponPassive
{
    private const string WeaponName = "Axe";
}
