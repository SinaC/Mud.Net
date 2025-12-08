using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups
{
    [Export(typeof(IAbilityGroup)), Shared]
    public class Benedictions : AbilityGroupBase
    {
        public Benedictions()
        {
            AddAbility("bless");
            AddAbility("calm");
            AddAbility("frenzy");
            AddAbility("holy word");
            AddAbility("remove curse");
        }

        #region IAbilityGroup

        public override string Name => "benedictions";

        #endregion
    }
}
