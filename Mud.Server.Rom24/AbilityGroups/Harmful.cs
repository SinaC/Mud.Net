using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups
{
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
}
