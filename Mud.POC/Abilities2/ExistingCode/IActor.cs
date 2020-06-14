namespace Mud.POC.Abilities2.ExistingCode
{
    public interface IActor
    {
        void Send(string format, params object[] args);
        void Act(ActOptions option, string format, params object[] arguments);
    }
}
