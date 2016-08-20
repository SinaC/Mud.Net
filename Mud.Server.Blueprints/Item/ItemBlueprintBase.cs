using System.Collections.Generic;
using System.Runtime.Serialization;
using Mud.Server.Constants;

namespace Mud.Server.Blueprints.Item
{
    [DataContract]
    public abstract class ItemBlueprintBase
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
        public WearLocations WearLocation { get; set; }
    }
}
