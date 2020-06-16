using System;

namespace Mud.Server.Interfaces.GameAction
{
    public interface IGameActionInfo : ICommandExecutionInfo
    {
        Type CommandExecutionType { get; }
    }
}
