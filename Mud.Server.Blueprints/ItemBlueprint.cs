using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mud.Server.Blueprints
{
    // TODO: sub item type
    [DataContract]
    public class ItemBlueprint
    {
        [DataMember]
        public int Id { get; set; }
        
        [DataMember]
        public string Name { get; set; }
        
        [DataMember]
        public string ShortDescription { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int Weight { get; set; }

        [DataMember]
        public int Cost { get; set; }

        [DataMember]
        public Dictionary<string, string> ExtraDescriptions { get; set; } // keyword -> description

        // TODO: flags, level, ...

        [DataMember]
        public int WearLocation { get; set; }

        [DataMember]
        public int[] Values { get; set; } // 5 entries
    }
}
