﻿using Mud.Server.Interfaces.Actor;

namespace Mud.Server.Interfaces.GameAction
{
    public interface IActionInput
    {
        IActor Actor { get; }
        string CommandLine { get; }
        string Command { get; }
        string RawParameters { get; }
        ICommandParameter[] Parameters { get; }
        IGameActionInfo GameActionInfo { get; }

        object Context { get; set; }

    }
}
