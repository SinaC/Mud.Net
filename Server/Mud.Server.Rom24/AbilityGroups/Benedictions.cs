using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups;

[Help(
@"Benedictions are priestly spells, used to bestow divine favor upon your allies.
They are often restricted to those of like alignment.  Only clerics and 
warriors may use these spells.")]
[OneLineHelp("powerful magics that grant the blessings of the gods")]
[Export(typeof(IAbilityGroup)), Shared]
public class Benedictions : AbilityGroupBase
{
    public Benedictions()
    {
        AddAbility("bless");
        AddAbility("calm");
        AddAbility("frenzy");
        AddAbility("holy word");
        AddAbility("remove curse");
    }

    #region IAbilityGroup

    public override string Name => "benedictions";

    #endregion
}
