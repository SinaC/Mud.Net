using Mud.Server.Commands;

namespace Mud.Server.Old.Commands.OutOfGame
{
    class Shutdown : IOutOfGameCommand
    {
        public string Name
        {
            get { return "shutdown"; }
        }

        public string Help
        {
            get { return "TODO"; }
        }

        public bool Execute(IPlayer player, string rawParameters, params CommandParameter[] parameters)
        {
            return true;
        }
    }
}
