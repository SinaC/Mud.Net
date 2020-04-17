using System.Collections.Generic;
using Mud.Domain;

namespace Mud.Server.Classes
{
    public class Druid : ClassBase
    {
        private readonly List<ResourceKinds> _rageOnly = new List<ResourceKinds>
        {
            Domain.ResourceKinds.Rage
        };

        private readonly List<ResourceKinds> _energyOnly = new List<ResourceKinds>
        {
            Domain.ResourceKinds.Energy
        };

        private readonly List<ResourceKinds> _manaOnly = new List<ResourceKinds>
        {
            Domain.ResourceKinds.Mana
        };

        #region IClass

        public override string Name => "druid";

        public override string ShortName => "Dru";

        public override IEnumerable<ResourceKinds> ResourceKinds { get; } = new List<ResourceKinds>
        {
            Domain.ResourceKinds.Mana, // others
            Domain.ResourceKinds.Energy, // cat form
            Domain.ResourceKinds.Rage // bear form
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

        public override int GetPrimaryAttributeByLevel(PrimaryAttributeTypes primaryAttribute, int level)
        {
            return level*10; // TODO: http://wow.gamepedia.com/Base_attributes
        }

        #endregion

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
