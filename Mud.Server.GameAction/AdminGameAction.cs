using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction
{
    public abstract class AdminGameAction : PlayerGameActionBase<IAdmin, IAdminGameActionInfo>
    {
        // TODO: min level ?
    }
}
