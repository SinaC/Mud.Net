using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.GameAction;

public abstract class PlayerGameAction : PlayerGameActionBase<IPlayer, IPlayerGameActionInfo>
{
}
