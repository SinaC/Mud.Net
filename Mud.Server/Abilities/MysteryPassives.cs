using System.Collections.Generic;

namespace Mud.Server.Abilities
{
    public partial class AbilityManager
    {
        [PassiveList]
        // ReSharper disable once UnusedMember.Global
        public IEnumerable<IAbility> Mysterty => new List<IAbility>
        {
            Passive(4000, "Dual wield"),
            Passive(4001, "Third wield"),
            Passive(4002, "Fourth wield"),
        };
    }
}
