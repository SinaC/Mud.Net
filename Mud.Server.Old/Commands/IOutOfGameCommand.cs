using Mud.Server.Commands;

namespace Mud.Server.Old.Commands
{
    public interface IOutOfGameCommand
    {
        string Name { get; }
        string Help { get; }

        bool Execute(IPlayer player, string rawParameters, params CommandParameter[] parameters); // return false if invalid, true if valid
    }
}
