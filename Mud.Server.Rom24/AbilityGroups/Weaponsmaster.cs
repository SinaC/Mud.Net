using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups
{
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
}
