using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.AbilityGroup.Interfaces;
using Mud.Server.Domain.Attributes;

namespace Mud.Server.Rom24.AbilityGroups;

[Help(
@"Warriors live for combat and the thrill of battle. They are the best fighters
of all the classes, but lack the subtle skills of thieves and the magical
talents of mages and priests.  Warriors are best for those who don't mind
taking the direct approach, even when another method might be called for.

Warriors begin with skill in the sword, and gain a second attack in combat.
Other weapon skills may be purchased cheaply, or gained in the default skill
package.")]
[Export(typeof(IAbilityGroup)), Shared]
public class WarriorDefault : AbilityGroupBase
{
    public WarriorDefault()
    {
        AddAbility("shield block");
        AddAbility("bash");
        AddAbility("disarm");
        AddAbility("enhanced damage");
        AddAbility("parry");
        AddAbility("rescue");
        AddAbility("third attack");

        AddAbilityGroup("weaponsmaster");
    }

    #region IAbilityGroup

    public override string Name => "warrior default";

    #endregion
}
