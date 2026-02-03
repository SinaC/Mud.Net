using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.StateMachine.Interfaces;

public interface IAvatarCreationStateMachine : IInputTrap<IPlayer>
{
    void Initialize(IPlayer player);
}

