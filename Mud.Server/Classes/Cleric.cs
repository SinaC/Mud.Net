using System.Collections.Generic;
using Mud.Server.Constants;

namespace Mud.Server.Classes
{
    public class Cleric : ClassBase
    {
        public override string Name => "cleric";

        public override string ShortName => "Cle";

        public override IEnumerable<ResourceKinds> ResourceKinds { get; } = new List<ResourceKinds>
        {
            Constants.ResourceKinds.Mana
        };

        public Cleric()
        {
            AddAbility(1, "renew");
            AddAbility(1, "power word: shield");
            AddAbility(1, "shadow Word: pain");
        }
    }
}
