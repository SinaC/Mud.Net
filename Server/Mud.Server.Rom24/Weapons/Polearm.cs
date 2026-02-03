using Mud.Server.Ability.Passive.Interfaces;
using Mud.Server.Domain.Attributes;

namespace Mud.Server.Rom24.Weapons;

[Weapon(WeaponName, ["Polearm"], LearnDifficultyMultiplier = 5)]
[Help(@"the use of pole weapons (except spears), including halberds")]
[OneLineHelp(@"the use of pole weapons (except spears), including halberds")]
public class Polearm : IWeaponPassive
{
    private const string WeaponName = "Polearm";
}
