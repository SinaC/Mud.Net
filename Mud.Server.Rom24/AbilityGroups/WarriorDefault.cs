using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups
{
    [Export(typeof(IAbilityGroup)), Shared]
    public class WarriorDefault : AbilityGroupBase
    {
        public WarriorDefault()
        {
            AddAbility("shield block");
            AddAbility("bash");
            AddAbility("disarm");
            AddAbility("enhanced damage");
            AddAbility("parry");
            AddAbility("rescue");
            AddAbility("third attack");

            AddAbilityGroup("weaponsmaster");
        }

        #region IAbilityGroup

        public override string Name => "warrior default";

        #endregion
    }
}
