using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups
{
    [Export(typeof(IAbilityGroup)), Shared]
    public class Beguiling : AbilityGroupBase
    {
        public Beguiling()
        {
            AddAbility("calm");
            AddAbility("charm person");
            AddAbility("sleep");
        }

        #region IAbilityGroup

        public override string Name => "beguiling";

        #endregion
    }
}
