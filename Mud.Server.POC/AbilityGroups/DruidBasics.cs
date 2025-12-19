using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.POC.AbilityGroups
{
    [Export(typeof(IAbilityGroup)), Shared]
    public class DruidBasics : AbilityGroupBase
    {
        public DruidBasics()
        {
            AddAbility("bear form");
            AddAbility("cat form");
            AddAbility("claw");
            AddAbility("rake");
            AddAbility("maul");
            AddAbility("demoralizing roar");
            AddAbility("ferocious bite");
            AddAbility("swipe");
        }

        #region IAbilityGroup

        public override string Name => "druid basics";

        #endregion
    }
}
