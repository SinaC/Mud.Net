using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups
{
    [Export(typeof(IAbilityGroup)), Shared]
    public class Detection : AbilityGroupBase
    {
        public Detection()
        {
            AddAbility("detect evil");
            AddAbility("detect good");
            AddAbility("detect hidden");
            AddAbility("detect invis");
            AddAbility("detect magic");
            AddAbility("detect poison");
            AddAbility("farsight");
            AddAbility("identify");
            AddAbility("know alignment");
            AddAbility("locate object");
        }

        #region IAbilityGroup

        public override string Name => "detection";

        #endregion
    }
}
