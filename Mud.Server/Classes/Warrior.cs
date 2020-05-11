using System.Collections.Generic;
using System.Linq;
using Mud.Domain;

namespace Mud.Server.Classes
{
    public class Warrior : ClassBase
    {
        #region IClass

        public override string Name => "warrior";

        public override string ShortName => "War";

        public override IEnumerable<ResourceKinds> ResourceKinds { get; } = Enumerable.Empty<ResourceKinds>();

        public override IEnumerable<ResourceKinds> CurrentResourceKinds(Forms form)
        {
            return ResourceKinds;
        }

        public override int MaxPracticePercentage => 75;

        public override int MinHitPointGainPerLevel => 11;

        public override int MaxHitPointGainPerLevel => 15;

        #endregion

        public Warrior()
        {
        }
    }
}
