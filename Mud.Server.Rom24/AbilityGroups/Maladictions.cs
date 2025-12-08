using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups
{
    [Export(typeof(IAbilityGroup)), Shared]
    public class Maladictions : AbilityGroupBase
    {
        public Maladictions()
        {
            AddAbility("blindness");
            AddAbility("change sex");
            AddAbility("curse");
            AddAbility("energy drain");
            AddAbility("plague");
            AddAbility("poison");
            AddAbility("slow");
            AddAbility("weaken");
        }

        #region IAbilityGroup

        public override string Name => "maladictions";

        #endregion
    }
}
