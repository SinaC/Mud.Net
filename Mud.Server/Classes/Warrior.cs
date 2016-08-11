using System.Collections.Generic;
using Mud.Server.Constants;

namespace Mud.Server.Classes
{
    public class Warrior : ClassBase
    {
        public override string Name => "warrior";

        public override string ShortName => "War";

        public override IEnumerable<ResourceKinds> ResourceKinds { get; } = new List<ResourceKinds>
        {
            Constants.ResourceKinds.Energy
        };

        public Warrior()
        {
            AddAbility(1, "parry");
            AddAbility(2, "shield block");
            AddAbility(10, "dodge");
            AddAbility(20, "battle shout");
        }
    }
}
