using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups
{
    [Export(typeof(IAbilityGroup)), Shared]
    public class MageDefault : AbilityGroupBase
    {
        public MageDefault()
        {
            AddAbility("lore");

            AddAbilityGroup("beguiling");
            AddAbilityGroup("combat");
            AddAbilityGroup("detection");
            AddAbilityGroup("enhancement");
            AddAbilityGroup("illusion");
            AddAbilityGroup("maladictions");
            AddAbilityGroup("protective");
            AddAbilityGroup("transportation");
            AddAbilityGroup("weather");
        }

        #region IAbilityGroup

        public override string Name => "mage default";

        #endregion
    }
}
