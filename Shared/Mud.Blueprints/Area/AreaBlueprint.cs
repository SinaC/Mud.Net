using System.Runtime.Serialization;
using Mud.Domain;

namespace Mud.Blueprints.Area;

[DataContract]
public class AreaBlueprint
{
    [DataMember]
    public int Id { get; set; }

    [DataMember]
    public string Filename { get; set; } = default!; // Filename

    [DataMember]
    public string Name { get; set; } = default!; // Name

    [DataMember]
    public string Credits { get; set; } = default!; // Credits

    [DataMember]
    public int MinId { get; set; } // Characters/Iems/Rooms id number range

    [DataMember]
    public int MaxId { get; set; }

    [DataMember]
    public string Builders { get; set; } = default!; // Builders

    [DataMember]
    public AreaFlags Flags { get; set; } = AreaFlags.None; // Flags

    [DataMember]
    public int Security { get; set; } // Security
}
