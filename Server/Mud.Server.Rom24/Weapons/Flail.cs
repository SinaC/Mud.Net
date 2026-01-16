using Mud.Server.Ability;
using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.Ability;

namespace Mud.Server.Rom24.Weapons;

[Weapon(WeaponName, ["Flail"], LearnDifficultyMultiplier = 5)]
[Help(@"skill in ball-and-chain type weapons")]
[OneLineHelp(@"skill in ball-and-chain type weapons")]
public class Flail : IWeaponPassive
{
    private const string WeaponName = "Flail";
}
