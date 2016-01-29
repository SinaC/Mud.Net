using System.Collections.Generic;
using Mud.Server.Constants;

namespace Mud.Server.Classes
{
    public class Thief : ClassBase
    {
        private readonly List<ResourceKinds> _resourceKinds = new List<ResourceKinds>
        {
            Constants.ResourceKinds.Energy
        };

        public override string Name
        {
            get { return "Thief"; }
        }

        public override string ShortName
        {
            get { return "Thi"; }
        }

        public override List<ResourceKinds> ResourceKinds
        {
            get { return _resourceKinds; }
        }

        public Thief()
        {
            AddAbility(1, "dodge");
            AddAbility(5, "rupture");
            AddAbility(10, "parry");
        }
    }
}
