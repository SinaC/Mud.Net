using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups;

[Help(
@"The enhancement spells improve upon the body's pontential, allowing feats of
superhuman strength and speed.  However, these spells may often have harmful
effects upon the recipient.")]
[OneLineHelp("spells that maximize physical potential, such as haste")]
[Export(typeof(IAbilityGroup)), Shared]
public class Enhancement : AbilityGroupBase
{
    public Enhancement()
    {
        AddAbility("giant strength");
        AddAbility("haste");
        AddAbility("infravision");
        AddAbility("refresh");
    }

    #region IAbilityGroup

    public override string Name => "enhancement";

    #endregion
}
