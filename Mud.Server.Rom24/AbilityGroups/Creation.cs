using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups
{
    [Export(typeof(IAbilityGroup)), Shared]
    public class Creation : AbilityGroupBase
    {
        public Creation()
        {
            AddAbility("continual light");
            AddAbility("create food");
            AddAbility("create spring");
            AddAbility("create water");
            AddAbility("create rose");
            AddAbility("floating disc");
        }

        #region IAbilityGroup

        public override string Name => "creation";

        #endregion
    }
}
