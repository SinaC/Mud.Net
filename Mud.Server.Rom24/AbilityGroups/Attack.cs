using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Common;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups;

[Help(
@"The attack spells are essentially powerful curses, fueled by divine energy
rather than magical power (like the combat spells are). They are less powerful
than the combat spells, but can still deal out a stinging blow.  Mages and 
thieves cannot use this group.")]
[OneLineHelp("a selection of offensive magics")]
[Export(typeof(IAbilityGroup)), Shared]
public class Attack : AbilityGroupBase
{
    public Attack()
    {
        AddAbility("demonfire");
        AddAbility("dispel evil");
        AddAbility("dispel good");
        AddAbility("earthquake");
        AddAbility("flamestrike");
        AddAbility("heat metal");
        AddAbility("ray of truth");
    }

    #region IAbilityGroup

    public override string Name => "attack";

    #endregion
}
