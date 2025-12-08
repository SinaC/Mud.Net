using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups
{
    [Export(typeof(IAbilityGroup)), Shared]
    public class Illusion : AbilityGroupBase
    {
        public Illusion()
        {
            AddAbility("invis");
            AddAbility("mass invis");
            AddAbility("ventriloquate");
        }

        #region IAbilityGroup

        public override string Name => "illusion";

        #endregion
    }
}
