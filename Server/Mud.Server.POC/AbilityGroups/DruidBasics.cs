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

            AddAbility("axe");
            AddAbility("dagger");
            AddAbility("flail");
            AddAbility("mace");
            AddAbility("polearm");
            AddAbility("spear");
            AddAbility("staff(weapon)");
            AddAbility("sword");
            AddAbility("whip");

            AddAbility("berserk");
            AddAbility("peek");
        }

        #region IAbilityGroup

        public override string Name => "druid basics";

        #endregion
    }
}
