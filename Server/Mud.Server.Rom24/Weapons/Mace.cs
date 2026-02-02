using Mud.Server.Ability.Passive.Interfaces;
using Mud.Server.Domain.Attributes;

namespace Mud.Server.Rom24.Weapons;

[Weapon(WeaponName, ["Mace"], LearnDifficultyMultiplier = 5)]
[Help(@"this skill includes clubs and hammers as well as maces")]
[OneLineHelp(@"this skill includes clubs and hammers as well as maces")]
public class Mace : IWeaponPassive
{
    private const string WeaponName = "Mace";
}
