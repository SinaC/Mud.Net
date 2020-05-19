using System.Runtime.Serialization;
using Mud.Domain;

namespace Mud.Server.Blueprints.Area
{
    [DataContract]
    public class AreaBlueprint
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Filename { get; set; } // Filename

        [DataMember]
        public string Name { get; set; } // Name

        [DataMember]
        public string Credits { get; set; } // Credits

        [DataMember]
        public int MinId { get; set; } // Characters/Iems/Rooms id number range

        [DataMember]
        public int MaxId { get; set; }

        [DataMember]
        public string Builders { get; set; } // Builders

        [DataMember]
        public AreaFlags Flags { get; set; } // Flags

        [DataMember]
        public int Security { get; set; } // Security
    }
}
