using Mud.Server.Ability;
using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.Ability;

namespace Mud.Server.Rom24.Weapons;

[Weapon(WeaponName, ["Mace"], LearnDifficultyMultiplier = 5)]
[Help(@"this skill includes clubs and hammers as well as maces")]
[OneLineHelp(@"this skill includes clubs and hammers as well as maces")]
public class Mace : IWeaponPassive
{
    private const string WeaponName = "Mace";
}
