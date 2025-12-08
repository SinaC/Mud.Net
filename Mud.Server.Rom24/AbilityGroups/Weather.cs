using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups
{
    [Export(typeof(IAbilityGroup)), Shared]
    public class Weather : AbilityGroupBase
    {
        public Weather()
        {
            AddAbility("call lightning");
            AddAbility("control weather");
            AddAbility("faerie fire");
            AddAbility("faerie fog");
            AddAbility("lightning bolt");
        }

        #region IAbilityGroup

        public override string Name => "weather";

        #endregion
    }
}
