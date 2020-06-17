namespace Mud.Server.Interfaces
{
    public interface IInputTrap<in TActor>
    {
        bool IsFinalStateReached { get; }

        void ProcessInput(TActor actor, string input);
    }
}
