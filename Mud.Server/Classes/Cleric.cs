using System.Collections.Generic;
using Mud.Server.Constants;

namespace Mud.Server.Classes
{
    public class Cleric : ClassBase
    {
        private readonly List<ResourceKinds> _resourceKinds = new List<ResourceKinds>
        {
            Constants.ResourceKinds.Mana
        };

        public override string Name
        {
            get { return "cleric"; }
        }

        public override string ShortName
        {
            get { return "Cle"; }
        }

        public override List<ResourceKinds> ResourceKinds
        {
            get { return _resourceKinds; }
        }

        public Cleric()
        {
            AddAbility(1, "renew");
            AddAbility(1, "power word: shield");
            AddAbility(1, "shadow Word: pain");
        }
    }
}
