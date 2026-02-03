using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.AbilityGroup.Interfaces;
using Mud.Server.Domain.Attributes;

namespace Mud.Server.Rom24.AbilityGroups;

[Help(
@"weaponsmaster	skill group of all weapons (save exotic weaponry)")]
[OneLineHelp("this group provides knowledge of all weapon types")]
[Export(typeof(IAbilityGroup)), Shared]
public class Weaponsmaster : AbilityGroupBase
{
    public Weaponsmaster()
    {
        AddAbility("axe");
        AddAbility("dagger");
        AddAbility("flail");
        AddAbility("mace");
        AddAbility("polearm");
        AddAbility("spear");
        AddAbility("sword");
        AddAbility("whip");
    }

    #region IAbilityGroup

    public override string Name => "weaponsmaster";

    #endregion
}
