using Mud.Domain;
using System.Runtime.Serialization;

namespace Mud.Server.Blueprints.Room;

[DataContract]
public class ExitBlueprint
{
    [DataMember]
    public ExitDirections Direction { get; set; }

    [DataMember]
    public string Description { get; set; } = default!;

    [DataMember]
    public string Keyword { get; set; } = default!;

    [DataMember]
    public ExitFlags Flags { get; set; } // flags

    [DataMember]
    public int Key { get; set; } // key item id (for locked door)

    [DataMember]
    public int Destination { get; set; } // destination room id
}
