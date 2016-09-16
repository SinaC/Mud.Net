using System;
using System.Runtime.Serialization;

namespace Mud.Server.Blueprints.Room
{
    [Flags]
    public enum ExitFlags
    {
        Door = 0x01,
        Closed = 0x02,
        Locked = 0x04,
        Easy = 0x08,
        Hard = 0x10,
        Hidden = 0x20,
    }

    [DataContract]
    public class ExitBlueprint
    {
        [DataMember]
        public string Description { get; set; }
        
        [DataMember]
        public string Keyword { get; set; }

        [DataMember]
        public ExitFlags Flags { get; set; } // flags

        [DataMember]
        public int Key { get; set; } // key item id (for locked door)

        [DataMember]
        public int Destination { get; set; } // destination room id
    }
}
