namespace Mud.Server
{
    public interface ICommandProcessor
    {
        void ProcessCommand(IClient client, string commandLine);
    }
}
