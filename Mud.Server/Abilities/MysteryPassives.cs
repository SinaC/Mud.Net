using System.Collections.Generic;

namespace Mud.Server.Abilities
{
    public partial class AbilityManager
    {
        [PassiveList]
        // ReSharper disable once UnusedMember.Global
        public IEnumerable<IAbility> Mysterty => new List<IAbility>
        {
            Passive(4000, "Dual wield")
        };
    }
}
