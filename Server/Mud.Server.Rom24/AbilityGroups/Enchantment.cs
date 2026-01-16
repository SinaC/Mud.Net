using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups;

[Help(
@"The enchantment spell group is used to imbue items with magical properties.
Currently, this spell group consists of enchant weapon and enchant armor, 
although more will be added in the future.  Only mages may enchant.")]
[Export(typeof(IAbilityGroup)), Shared]
public class Enchantment : AbilityGroupBase
{
    public Enchantment()
    {
        AddAbility("enchant armor");
        AddAbility("enchant weapon");
        AddAbility("fireproof");
        AddAbility("recharge");
    }

    #region IAbilityGroup

    public override string Name => "enchantment";

    #endregion
}
