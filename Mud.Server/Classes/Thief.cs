using System.Collections.Generic;
using Mud.Server.Constants;

namespace Mud.Server.Classes
{
    public class Thief : ClassBase
    {
        public override string Name => "Thief";

        public override string ShortName => "Thi";

        public override IEnumerable<ResourceKinds> ResourceKinds { get; } = new List<ResourceKinds>
        {
            Constants.ResourceKinds.Energy
        };

        public Thief()
        {
            AddAbility(1, "dodge");
            AddAbility(5, "rupture");
            AddAbility(10, "parry");
        }
    }
}
