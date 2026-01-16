using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups;

[Help(
@"The detection spells have may uses, all related to gathering information. They
can be used to see hidden objects, give information about treasure, or even
scry out the true nature of your foe.  All classes except warriors can use 
detection spells.")]
[OneLineHelp("informational magics, such as detect magic and identify")]
[Export(typeof(IAbilityGroup)), Shared]
public class Detection : AbilityGroupBase
{
    public Detection()
    {
        AddAbility("detect evil");
        AddAbility("detect good");
        AddAbility("detect hidden");
        AddAbility("detect invis");
        AddAbility("detect magic");
        AddAbility("detect poison");
        AddAbility("farsight");
        AddAbility("identify");
        AddAbility("know alignment");
        AddAbility("locate object");
    }

    #region IAbilityGroup

    public override string Name => "detection";

    #endregion
}
