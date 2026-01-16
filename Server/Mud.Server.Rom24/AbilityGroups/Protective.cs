using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups;

[Help(
@"The protective spells are used to shield against harm, whether from spells or
physical attack.  They range from the weak armor spell to the sought-after
sanctuary charm.  Also included in this group are several spells for dispeling
hostile magics. Any class may use this group.")]
[OneLineHelp("defensive magics, including the powerful sanctuary spell")]
[Export(typeof(IAbilityGroup)), Shared]
public class Protective : AbilityGroupBase
{
    public Protective()
    {
        AddAbility("armor");
        AddAbility("cancellation");
        AddAbility("dispel magic");
        AddAbility("fireproof");
        AddAbility("protection evil");
        AddAbility("protection good");
        AddAbility("sanctuary");
        AddAbility("shield");
        AddAbility("stone skin");
    }

    #region IAbilityGroup

    public override string Name => "protective";

    #endregion
}
