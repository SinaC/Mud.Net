using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups
{
    [Export(typeof(IAbilityGroup)), Shared]
    public class Healing : AbilityGroupBase
    {
        public Healing()
        {
            AddAbility("cure critical");
            AddAbility("cure light");
            AddAbility("cure serious");
            AddAbility("heal");
            AddAbility("mass healing");
            AddAbility("refresh");
        }

        #region IAbilityGroup

        public override string Name => "healing";

        #endregion
    }
}
