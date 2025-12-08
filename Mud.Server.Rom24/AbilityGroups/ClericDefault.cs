using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups
{
    [Export(typeof(IAbilityGroup)), Shared]
    public class ClericDefault : AbilityGroupBase
    {
        public ClericDefault()
        {
            AddAbility("flail");
            AddAbility("shield block");

            AddAbilityGroup("attack");
            AddAbilityGroup("creation");
            AddAbilityGroup("curative");
            AddAbilityGroup("benedictions");
            AddAbilityGroup("detection");
            AddAbilityGroup("healing");
            AddAbilityGroup("maladictions");
            AddAbilityGroup("protective");
            AddAbilityGroup("transportation");
            AddAbilityGroup("weather");
        }

        #region IAbilityGroup

        public override string Name => "cleric default";

        #endregion
    }
}
