using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups
{
    [Export(typeof(IAbilityGroup)), Shared]
    public class Protective : AbilityGroupBase
    {
        public Protective()
        {
            AddAbility("armor");
            AddAbility("cancellation");
            AddAbility("dispel magic");
            AddAbility("fireproof");
            AddAbility("protection evil");
            AddAbility("protection good");
            AddAbility("sanctuary");
            AddAbility("shield");
            AddAbility("stone skin");
        }

        #region IAbilityGroup

        public override string Name => "protective";

        #endregion
    }
}
