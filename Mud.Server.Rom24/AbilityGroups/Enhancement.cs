using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups
{
    [Export(typeof(IAbilityGroup)), Shared]
    public class Enhancement : AbilityGroupBase
    {
        public Enhancement()
        {
            AddAbility("giant strength");
            AddAbility("haste");
            AddAbility("infravision");
            AddAbility("refresh");
        }

        #region IAbilityGroup

        public override string Name => "enhancement";

        #endregion
    }
}
