using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mud.Server.Blueprints
{
    [DataContract]
    public class ObjectBlueprint
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
        public Dictionary<string, string> ExtraDescriptions; // keyword -> description

        // TODO: flags, level, ...
    }
}
