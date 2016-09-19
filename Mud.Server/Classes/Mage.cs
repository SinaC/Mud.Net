using System.Collections.Generic;
using Mud.Server.Constants;

namespace Mud.Server.Classes
{
    public class Mage : ClassBase
    {
        #region IClass

        public override string Name => "mage";

        public override string ShortName => "Mag";

        public override IEnumerable<ResourceKinds> ResourceKinds { get; } = new List<ResourceKinds>
        {
            Constants.ResourceKinds.Mana
        };

        public override IEnumerable<ResourceKinds> CurrentResourceKinds(Forms form)
        {
            return ResourceKinds; // always mana
        }

        public override int GetPrimaryAttributeByLevel(PrimaryAttributeTypes primaryAttribute, int level)
        {
            return level * 10; // TODO: http://wow.gamepedia.com/Base_attributes
        }


        #endregion

        public Mage()
        {
            AddAbility(1, "wrath");
            AddAbility(20, "dodge");
        }
    }
}
