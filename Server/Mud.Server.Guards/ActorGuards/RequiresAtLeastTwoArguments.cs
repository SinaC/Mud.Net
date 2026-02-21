using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Actor;

namespace Mud.Server.Guards.ActorGuards;

public class RequiresAtLeastTwoArguments : RequiresAtLeastArgumentBase, IGuard<IActor>
{
    public RequiresAtLeastTwoArguments() : base(2)
    {
    }
}
