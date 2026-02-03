using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.AbilityGroup.Interfaces;

namespace Mud.Server.Rom24.AbilityGroups;

[Export(typeof(IAbilityGroup)), Shared]
public class ThiefBasics : AbilityGroupBase
{
    public ThiefBasics()
    {
        AddAbility("dagger");
        AddAbility("steal");
    }

    #region IAbilityGroup

    public override string Name => "thief basics";

    #endregion
}
