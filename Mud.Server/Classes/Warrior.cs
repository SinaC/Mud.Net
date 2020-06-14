using System.Collections.Generic;
using Mud.Domain;

namespace Mud.Server.Classes
{
    public class Warrior : ClassBase
    {
        #region IClass

        public override string Name => "warrior";

        public override string ShortName => "War";

        public override IEnumerable<ResourceKinds> ResourceKinds { get; } = new List<ResourceKinds>
        {
            Domain.ResourceKinds.Rage
        };

        public override IEnumerable<ResourceKinds> CurrentResourceKinds(Forms form)
        {
            return ResourceKinds; // always rage
        }

        public override int GetPrimaryAttributeByLevel(PrimaryAttributeTypes primaryAttribute, int level)
        {
            return level * 10; // TODO: http://wow.gamepedia.com/Base_attributes
        }

        #endregion

        public Warrior()
        {
            AddAbility(1, "parry");
            AddAbility(2, "shield block");
            AddAbility(10, "dodge");
            AddAbility(20, "battle shout");
            AddAbility(50, "rupture"); // test
        }
    }
}
