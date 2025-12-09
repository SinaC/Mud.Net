using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Common;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups;

[Help(
@"The creation spell group is used to create objects, of temporary or permanent
duration.  Skill creators can travel without food or drink, and use their 
powers to create nourishment as required.")]
[OneLineHelp("the making of physical objects, such as food and water")]
[Export(typeof(IAbilityGroup)), Shared]
public class Creation : AbilityGroupBase
{
    public Creation()
    {
        AddAbility("continual light");
        AddAbility("create food");
        AddAbility("create spring");
        AddAbility("create water");
        AddAbility("create rose");
        AddAbility("floating disc");
    }

    #region IAbilityGroup

    public override string Name => "creation";

    #endregion
}
