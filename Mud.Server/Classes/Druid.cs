using System.Collections.Generic;
using Mud.Server.Constants;

namespace Mud.Server.Classes
{
    public class Druid : ClassBase
    {
        private readonly List<ResourceKinds> _rageOnly = new List<ResourceKinds>
        {
            Constants.ResourceKinds.Rage
        };
        private readonly List<ResourceKinds> _energyOnly = new List<ResourceKinds>
        {
            Constants.ResourceKinds.Energy
        };
        private readonly List<ResourceKinds> _manaOnly = new List<ResourceKinds>
        {
            Constants.ResourceKinds.Mana
        };

        public override string Name => "Druid";
        public override string ShortName => "Dru";
        public override IEnumerable<ResourceKinds> ResourceKinds { get; } = new List<ResourceKinds>
        {
            Constants.ResourceKinds.Mana, // others
            Constants.ResourceKinds.Energy, // cat form
            Constants.ResourceKinds.Rage // bear form
        };

        public override IEnumerable<ResourceKinds> CurrentResourceKinds(Forms form)
        {
            switch (form)
            {
                case Forms.Bear:
                    return _rageOnly;
                case Forms.Cat:
                    return _energyOnly;
                default:
                    return _manaOnly;
            }
        }

        public Druid()
        {
            AddAbility(1, "thrash(cat)");
            AddAbility(1, "bear form");
            AddAbility(10, "cat form");
            AddAbility(20, "dodge");
            AddAbility(20, "swiftmend");
        }
    }
}
