using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups;

[Help(
@"Clerics are the most defensively orientated of all the classes.  Most of their
spells focus on healing or defending the faithful, with their few combat spells
being far less powerful than those of mages. However, clerics are the best 
class by far at healing magics, and they posess an impressive area of
protective magics, as well as fair combat prowess.

All clerics begin with skill in the mace.  Other weapon or shield skills must
be purchased, many at a very dear cost.")]
[Export(typeof(IAbilityGroup)), Shared]
public class ClericDefault : AbilityGroupBase
{
    public ClericDefault()
    {
        AddAbility("flail");
        AddAbility("shield block");

        AddAbilityGroup("attack");
        AddAbilityGroup("creation");
        AddAbilityGroup("curative");
        AddAbilityGroup("benedictions");
        AddAbilityGroup("detection");
        AddAbilityGroup("healing");
        AddAbilityGroup("maladictions");
        AddAbilityGroup("protective");
        AddAbilityGroup("transportation");
        AddAbilityGroup("weather");
    }

    #region IAbilityGroup

    public override string Name => "cleric default";

    #endregion
}
