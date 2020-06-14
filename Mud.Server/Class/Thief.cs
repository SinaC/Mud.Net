using System.Collections.Generic;
using System.Linq;
using Mud.Domain;

namespace Mud.Server.Class
{
    public class Thief : ClassBase
    {
        #region IClass

        public override string Name => "thief";

        public override string ShortName => "Thi";

        public override IEnumerable<ResourceKinds> ResourceKinds { get; } = Enumerable.Empty<ResourceKinds>();

        public override IEnumerable<ResourceKinds> CurrentResourceKinds(Forms form)
        {
            return ResourceKinds;
        }

        public override BasicAttributes PrimeAttribute => BasicAttributes.Dexterity;

        public override int MaxPracticePercentage => 75;

        public override (int thac0_00, int thac0_32) Thac0 => (20, -4);

        public override int MinHitPointGainPerLevel => 8;

        public override int MaxHitPointGainPerLevel => 13;

        #endregion

        public Thief()
        {
        }
    }
}
