using Mud.Flags.Interfaces;

namespace Mud.Blueprints.Area;

public class AreaBlueprint
{
    public int Id { get; set; }

    public string Filename { get; set; } = default!; // Filename

    public string Name { get; set; } = default!; // Name

    public string Credits { get; set; } = default!; // Credits

    public int MinId { get; set; } // Characters/Iems/Rooms id number range

    public int MaxId { get; set; }

    public string Builders { get; set; } = default!; // Builders

    public IAreaFlags Flags { get; set; } = default!; // Flags

    public int Security { get; set; } // Security
}
