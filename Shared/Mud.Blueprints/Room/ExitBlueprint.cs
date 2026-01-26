using Mud.Domain;
using Mud.Flags.Interfaces;

namespace Mud.Blueprints.Room;

public class ExitBlueprint
{
    public ExitDirections Direction { get; set; }

    public string Description { get; set; } = default!;

    public string Keyword { get; set; } = default!;

    public IExitFlags Flags { get; set; } = default!; // flags

    public int Key { get; set; } // key item id (for locked door)

    public int Destination { get; set; } // destination room id
}
