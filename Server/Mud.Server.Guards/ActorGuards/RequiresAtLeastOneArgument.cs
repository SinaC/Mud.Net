using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Actor;

namespace Mud.Server.Guards.ActorGuards;

public class RequiresAtLeastOneArgument : RequiresAtLeastArgumentBase, IGuard<IActor>
{
    public RequiresAtLeastOneArgument() : base(1)
    {
    }
}
