using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.AbilityGroup.Interfaces;
using Mud.Server.Domain.Attributes;

namespace Mud.Server.Rom24.AbilityGroups;

[Help(
@"Every character starts with two skill groups, one for their class and a default
set that all characters receive.")]
[Export(typeof(IAbilityGroup)), Shared]
public class RomBasics : AbilityGroupBase
{
    public RomBasics()
    {
        AddAbility("scrolls");
        AddAbility("staves");
        AddAbility("wands");
        AddAbility("recall");
    }

    #region IAbilityGroup

    public override string Name => "rom basics";

    #endregion
}
