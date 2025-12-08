using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups
{
    [Export(typeof(IAbilityGroup)), Shared]
    public class Draconian : AbilityGroupBase
    {
        public Draconian()
        {
            AddAbility("acid breath");
            AddAbility("fire breath");
            AddAbility("frost breath");
            AddAbility("gas breath");
            AddAbility("lightning breath");
        }

        #region IAbilityGroup

        public override string Name => "draconian";

        #endregion
    }
}
