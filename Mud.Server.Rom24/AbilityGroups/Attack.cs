using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups
{
    [Export(typeof(IAbilityGroup)), Shared]
    public class Attack : AbilityGroupBase
    {
        public Attack()
        {
            AddAbility("demonfire");
            AddAbility("dispel evil");
            AddAbility("dispel good");
            AddAbility("earthquake");
            AddAbility("flamestrike");
            AddAbility("heat metal");
            AddAbility("ray of truth");
        }

        #region IAbilityGroup

        public override string Name => "attack";

        #endregion
    }
}
