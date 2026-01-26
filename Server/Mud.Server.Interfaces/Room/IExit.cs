using Mud.Blueprints.Room;
using Mud.Flags.Interfaces;

namespace Mud.Server.Interfaces.Room;

public interface IExit : ICloseable
{
    ExitBlueprint Blueprint { get; }

    string Name { get; } // should be equal to first word of keywords in blueprint
    IEnumerable<string> Keywords { get; }
    string Description { get; }
    IRoom Destination { get; }
    IExitFlags ExitFlags { get; }

    bool IsDoor { get; }
    bool IsHidden { get; }

    void OnRemoved();
}
