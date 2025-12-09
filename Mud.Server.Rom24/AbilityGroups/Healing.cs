using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Common;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups;

[Help(
@"The healing spells are used to cure the wounds that adventurers inevitably
suffer in battle.  For curing other conditions, such as poison, see the
curative spell group.  Only warriors and priests have access to this group.")]
[OneLineHelp("spells for treating wounds, from scratches to death blows")]
[Export(typeof(IAbilityGroup)), Shared]
public class Healing : AbilityGroupBase
{
    public Healing()
    {
        AddAbility("cure critical");
        AddAbility("cure light");
        AddAbility("cure serious");
        AddAbility("heal");
        AddAbility("mass healing");
        AddAbility("refresh");
    }

    #region IAbilityGroup

    public override string Name => "healing";

    #endregion
}
