using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups;

[Help(
@"The Draconian spell group deals with the magic of dragons -- in this case, 
bringing forth the devastating power of their breath weapons upon your foes.
Only mages have the mental training necessary to cast these spells, and few
of them ever reach the level of mastery required to use them.")]
[Export(typeof(IAbilityGroup)), Shared]
public class Draconian : AbilityGroupBase
{
    public Draconian()
    {
        AddAbility("acid breath");
        AddAbility("fire breath");
        AddAbility("frost breath");
        AddAbility("gas breath");
        AddAbility("lightning breath");
    }

    #region IAbilityGroup

    public override string Name => "draconian";

    #endregion
}
