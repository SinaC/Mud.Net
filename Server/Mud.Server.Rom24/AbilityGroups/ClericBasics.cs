using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups;

[Export(typeof(IAbilityGroup)), Shared]
public class ClericBasics : AbilityGroupBase
{
    public ClericBasics()
    {
        AddAbility("mace");
    }

    #region IAbilityGroup

    public override string Name => "cleric basics";

    #endregion
}
