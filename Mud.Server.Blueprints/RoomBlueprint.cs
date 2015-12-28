using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mud.Server.Blueprints
{
    [DataContract]
    public class RoomBlueprint
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public Dictionary<string, string> ExtraDescriptions { get; set; } // keyword -> description

        [DataMember]
        public ExitBlueprint[] Exits { get; set; } // TODO: fixed length or list (+ add direction in ExitBlueprint)

        // TODO: flags, healrate, sector, ...
    }
}
