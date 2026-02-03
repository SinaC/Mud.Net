using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.AbilityGroup.Interfaces;

namespace Mud.Server.Rom24.AbilityGroups;

[Export(typeof(IAbilityGroup)), Shared]
public class MageBasics : AbilityGroupBase
{
    public MageBasics()
    {
        AddAbility("dagger");
    }

    #region IAbilityGroup

    public override string Name => "mage basics";

    #endregion
}
