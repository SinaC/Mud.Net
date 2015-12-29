using System.Runtime.Serialization;

namespace Mud.Server.Blueprints
{
    [DataContract]
    public class ExitBlueprint
    {
        [DataMember]
        public string Description { get; set; }
        
        [DataMember]
        public string Keyword { get; set; }

        //[DataMember]
        //public long ExitInfo { get; set; } // flags

        [DataMember]
        public int Key { get; set; } // key item id

        [DataMember]
        public int Destination { get; set; } // destination room id
    }
}
