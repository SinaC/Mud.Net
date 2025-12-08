using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups
{
    [Export(typeof(IAbilityGroup)), Shared]
    public class Curative : AbilityGroupBase
    {
        public Curative()
        {
            AddAbility("cure blindness");
            AddAbility("cure disease");
            AddAbility("cure poison");
        }

        #region IAbilityGroup

        public override string Name => "curative";

        #endregion
    }
}
