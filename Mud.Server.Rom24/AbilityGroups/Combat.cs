using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups
{
    [Export(typeof(IAbilityGroup)), Shared]
    public class Combat : AbilityGroupBase
    {
        public Combat()
        {
            AddAbility("acid blast");
            AddAbility("burning hands");
            AddAbility("chain lightning");
            AddAbility("chill touch");
            AddAbility("colour spray");
            AddAbility("fireball");
            AddAbility("lightning bolt");
            AddAbility("magic missile");
            AddAbility("shocking grasp");
        }

        #region IAbilityGroup

        public override string Name => "combat";

        #endregion
    }
}
