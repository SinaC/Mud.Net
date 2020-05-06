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

        public override int MaxPracticePercentage => 75;

        public override int GetAttributeByLevel(CharacterAttributes attribute, int level)
        {
            return level * 10; // TODO: http://wow.gamepedia.com/Base_attributes
        }

        #endregion

        public Warrior()
        {
        }
    }
}
