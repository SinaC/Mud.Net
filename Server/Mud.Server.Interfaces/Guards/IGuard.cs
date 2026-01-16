using Mud.Server.Interfaces.Actor;

namespace Mud.Server.Interfaces.Guards;

public interface IGuard<TActor>
    where TActor: IActor
{
    string? Guards(TActor actor);
}
