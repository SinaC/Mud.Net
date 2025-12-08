using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups
{
    [Export(typeof(IAbilityGroup)), Shared]
    public class Enchantment : AbilityGroupBase
    {
        public Enchantment()
        {
            AddAbility("enchant armor");
            AddAbility("enchant weapon");
            AddAbility("fireproof");
            AddAbility("recharge");
        }

        #region IAbilityGroup

        public override string Name => "enchantment";

        #endregion
    }
}
