using Mud.Server.Ability;
using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.Ability;

namespace Mud.Server.Rom24.Weapons;

[Weapon(WeaponName, ["Polearm"], LearnDifficultyMultiplier = 5)]
[Help(@"the use of pole weapons (except spears), including halberds")]
[OneLineHelp(@"the use of pole weapons (except spears), including halberds")]
public class Polearm : IWeaponPassive
{
    private const string WeaponName = "Polearm";
}
