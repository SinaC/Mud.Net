using System.Collections.Generic;
using Mud.Server.Constants;

namespace Mud.Server.Classes
{
    public class Mage : ClassBase
    {
        private readonly List<ResourceKinds> _resourceKinds = new List<ResourceKinds>
        {
            Constants.ResourceKinds.Mana
        };

        public override string Name
        {
            get { return "mage"; }
        }

        public override string ShortName
        {
            get { return "Mag"; }
        }

        public override List<ResourceKinds> ResourceKinds
        {
            get { return _resourceKinds; }
        }

        public Mage()
        {
            AddAbility(1, "wrath");
            AddAbility(20, "dodge");
        }
    }
}
