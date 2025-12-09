using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Common;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups;

[Help(
@"Mages specialize in the casting of spells, offensive ones in particular.
Mages have the highest-powered magic of any class, and are the only classes
able to use the draconian and enchanting spell groups.  They are also very
skilled at the use of magical items, though their combat skills are the 
weakest of any class.

All mages begin with skill in the dagger. Any other weapon skills must be
purchased, at a very high rate.")]
[Export(typeof(IAbilityGroup)), Shared]
public class MageDefault : AbilityGroupBase
{
    public MageDefault()
    {
        AddAbility("lore");

        AddAbilityGroup("beguiling");
        AddAbilityGroup("combat");
        AddAbilityGroup("detection");
        AddAbilityGroup("enhancement");
        AddAbilityGroup("illusion");
        AddAbilityGroup("maladictions");
        AddAbilityGroup("protective");
        AddAbilityGroup("transportation");
        AddAbilityGroup("weather");
    }

    #region IAbilityGroup

    public override string Name => "mage default";

    #endregion
}
