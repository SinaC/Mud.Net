using System.Collections.Generic;
using Mud.Server.Constants;

namespace Mud.Server.Classes
{
    public class Druid : ClassBase
    {
        public override string Name => "Druid";
        public override string ShortName => "Dru";
        public override IEnumerable<ResourceKinds> ResourceKinds { get; } = new List<ResourceKinds>
        {
            Constants.ResourceKinds.Mana, // normal, moonkin, tree, cheetah
            Constants.ResourceKinds.Energy, // cat
            Constants.ResourceKinds.Rage // bear
        };
    }
}
