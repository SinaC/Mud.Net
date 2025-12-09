using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Common;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups;

[Help(
@"This spell group is the opposite of healing.  The harmful spells are designed
to tear flesh from bone, rupture arteries, and generally turn the body against
itself.  Only priests and warriors may use this spell group.")]
[Export(typeof(IAbilityGroup)), Shared]
public class Harmful : AbilityGroupBase
{
    public Harmful()
    {
        AddAbility("cause critical");
        AddAbility("cause light");
        AddAbility("cause serious");
        AddAbility("harm");
    }

    #region IAbilityGroup

    public override string Name => "harmful";

    #endregion
}
