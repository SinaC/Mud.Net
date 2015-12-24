namespace Mud.Server.Commands
{
    public interface IInGameCommand
    {
        string Name { get; }
        string Help { get; }

        bool Execute(IEntity entity, string rawParameters, params CommandParameter[] parameters); // return false if invalid, true if valid
    }
}
