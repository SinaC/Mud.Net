namespace Mud.Server.Commands
{
    public interface ICommandProcessor
    {
        bool ProcessCommand(IEntity entity, string commandLine); // IG
        bool ProcessCommand(IPlayer player, string commandLine); // OOG
    }
}
