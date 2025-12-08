using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups
{
    [Export(typeof(IAbilityGroup)), Shared]
    public class WarriorBasics : AbilityGroupBase
    {
        public WarriorBasics()
        {
            AddAbility("sword");
            AddAbility("second attack");
        }

        #region IAbilityGroup

        public override string Name => "warrior basics";

        #endregion
    }
}
