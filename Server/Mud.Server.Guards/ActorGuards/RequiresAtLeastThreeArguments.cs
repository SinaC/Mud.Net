namespace Mud.Server.Guards.ActorGuards;

public class RequiresAtLeastThreeArguments : RequiresAtLeastArgumentBase
{
    public RequiresAtLeastThreeArguments() : base(3)
    {
    }
}
