using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups
{
    [Export(typeof(IAbilityGroup)), Shared]
    public class Transportation : AbilityGroupBase
    {
        public Transportation()
        {
            AddAbility("fly");
            AddAbility("gate");
            AddAbility("nexus");
            AddAbility("pass door");
            AddAbility("portal");
            AddAbility("summon");
            AddAbility("teleport");
            AddAbility("word of recall");
        }

        #region IAbilityGroup

        public override string Name => "transportation";

        #endregion
    }
}
