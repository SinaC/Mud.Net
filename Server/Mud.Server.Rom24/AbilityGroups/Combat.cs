using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups;

[Help(
@"These sorcerous spells are, in the words of one famous mage, just 'more ways to
toss energy around'.  They are the most powerful of the damaging spells, and
considered an essential part of most wizards' collections. Clerics and thieves
do not have access to these spells.")]
[OneLineHelp("offensive magics, such as fireball and chill touch")]
[Export(typeof(IAbilityGroup)), Shared]
public class Combat : AbilityGroupBase
{
    public Combat()
    {
        AddAbility("acid blast");
        AddAbility("burning hands");
        AddAbility("chain lightning");
        AddAbility("chill touch");
        AddAbility("colour spray");
        AddAbility("fireball");
        AddAbility("lightning bolt");
        AddAbility("magic missile");
        AddAbility("shocking grasp");
    }

    #region IAbilityGroup

    public override string Name => "combat";

    #endregion
}
