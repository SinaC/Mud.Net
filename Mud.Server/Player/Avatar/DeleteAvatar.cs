using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Player.Avatar;

[PlayerCommand("deleteavatar", "Avatar", CannotBeImpersonated = true)]
[Syntax("[cmd] <avatar name>")]
public class DeleteAvatar : PlayerGameAction
{
    public override void Execute(IActionInput actionInput)
    {
        //TODO UniquenessManager.RemoveAvatarName(avatarName)
        throw new NotImplementedException();
    }
}
