using System.Collections.Generic;
using Mud.Server.Constants;

namespace Mud.Server.Classes
{
    public class Warrior : ClassBase
    {
        private readonly List<ResourceKinds> _resourceKinds = new List<ResourceKinds>
        {
            Constants.ResourceKinds.Energy
        };

        public override string Name
        {
            get { return "warrior"; }
        }

        public override string ShortName
        {
            get { return "War"; }
        }

        public override List<ResourceKinds> ResourceKinds
        {
            get { return _resourceKinds; }
        }

        public Warrior()
        {
            AddAbility(1, "parry");
            AddAbility(2, "shield block");
            AddAbility(10, "dodge");
            AddAbility(20, "battle shout");
        }
    }
}
