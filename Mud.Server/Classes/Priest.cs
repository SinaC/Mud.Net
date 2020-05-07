using System.Collections.Generic;
using Mud.Domain;

namespace Mud.Server.Classes
{
    public class Priest : ClassBase
    {
        #region IClass

        public override string Name => "priest";

        public override string ShortName => "Pri";

        public override IEnumerable<ResourceKinds> ResourceKinds { get; } = new List<ResourceKinds>
        {
            Domain.ResourceKinds.Mana
        };

        public override IEnumerable<ResourceKinds> CurrentResourceKinds(Forms form)
        {
            return ResourceKinds; // always mana
        }

        public override int MaxPracticePercentage => 75;

        public override int GetAttributeByLevel(CharacterAttributes attribute, int level)
        {
            return level * 10; // TODO: http://wow.gamepedia.com/Base_attributes
        }

        #endregion

        public Priest()
        {
        }
    }
}
