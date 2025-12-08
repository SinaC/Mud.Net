using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups
{
    [Export(typeof(IAbilityGroup)), Shared]
    public class ThiefDefault : AbilityGroupBase
    {
        public ThiefDefault()
        {
            AddAbility("mace");
            AddAbility("sword");
            AddAbility("backstab");
            AddAbility("disarm");
            AddAbility("dodge");
            AddAbility("second attack");
            AddAbility("trip");
            AddAbility("hide");
            AddAbility("peek");
            AddAbility("pick lock");
            AddAbility("sneak");
        }

        #region IAbilityGroup

        public override string Name => "thief default";

        #endregion
    }
}
