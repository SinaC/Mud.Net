using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups
{
    [Export(typeof(IAbilityGroup)), Shared]
    public class RomBasics : AbilityGroupBase
    {
        public RomBasics()
        {
            AddAbility("scrolls");
            AddAbility("staves");
            AddAbility("wands");
            AddAbility("recall");
        }

        #region IAbilityGroup

        public override string Name => "rom basics";

        #endregion
    }
}
