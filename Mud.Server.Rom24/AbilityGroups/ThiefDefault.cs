using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Common;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups;

[Help(
@"Thieves are a marginal class. They do few things better than any other class,
but have the widest range of skills available.  Thieves are specialists at
thievery and covert actions, being capable of entering areas undetected where
more powerful adventurers would fear to tread.  They are better fighters than
clerics, but lack the wide weapon selection of warriors.

All thieves begin with the dagger combat skill, and are learned in steal as 
well.  Any other weapon skills must be purchased, unless the default selection
is chosen.")]
[Export(typeof(IAbilityGroup)), Shared]
public class ThiefDefault : AbilityGroupBase
{
    public ThiefDefault()
    {
        AddAbility("mace");
        AddAbility("sword");
        AddAbility("backstab");
        AddAbility("disarm");
        AddAbility("dodge");
        AddAbility("second attack");
        AddAbility("trip");
        AddAbility("hide");
        AddAbility("peek");
        AddAbility("pick lock");
        AddAbility("sneak");
    }

    #region IAbilityGroup

    public override string Name => "thief default";

    #endregion
}
