using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.AbilityGroup.Interfaces;
using Mud.Server.Domain.Attributes;

namespace Mud.Server.Rom24.AbilityGroups;

[Help(
@"This insideous spell group is used to invade the minds of others.  It consists
of various charming magics, well suited for turning your foes to your cause.
Only mages and thieves have access to the beguiling magics.")]
[OneLineHelp("spells that control the mind")]
[Export(typeof(IAbilityGroup)), Shared]
public class Beguiling : AbilityGroupBase
{
    public Beguiling()
    {
        AddAbility("calm");
        AddAbility("charm person");
        AddAbility("sleep");
    }

    #region IAbilityGroup

    public override string Name => "beguiling";

    #endregion
}
