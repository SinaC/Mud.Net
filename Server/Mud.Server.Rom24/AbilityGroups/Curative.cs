using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups;

[Help(
@"The curative spells are used to heal various unpleasant conditions that can
befall an adventurer.   For healing of damage, see the healing spell group.
Curative spells cannot be used by mages or thieves.")]
[OneLineHelp("spells that cure the sick and feeble of their ailments")]
[Export(typeof(IAbilityGroup)), Shared]
public class Curative : AbilityGroupBase
{
    public Curative()
    {
        AddAbility("cure blindness");
        AddAbility("cure disease");
        AddAbility("cure poison");
    }

    #region IAbilityGroup

    public override string Name => "curative";

    #endregion
}
