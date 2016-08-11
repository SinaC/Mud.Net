using System.Collections.Generic;
using Mud.Server.Constants;

namespace Mud.Server.Classes
{
    public class Mage : ClassBase
    {
        public override string Name => "mage";

        public override string ShortName => "Mag";

        public override IEnumerable<ResourceKinds> ResourceKinds { get; } = new List<ResourceKinds>
        {
            Constants.ResourceKinds.Mana
        };

        public Mage()
        {
            AddAbility(1, "wrath");
            AddAbility(20, "dodge");
        }
    }
}
