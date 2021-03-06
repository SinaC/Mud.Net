﻿namespace Mud.Server.Interfaces.GameAction
{
    public interface IGameAction : IRegistrable
    {
        // Guards the action against incorrect usage
        // Returns null if all guard pass
        // Returns error message describing failure
        string Guards(IActionInput actionInput);
        // Execute the action, Guards must be called before
        void Execute(IActionInput actionInput);
    }
}
