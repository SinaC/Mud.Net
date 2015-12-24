using Mud.Logger;

namespace Mud.Server.Commands
{
    class Shutdown : ICommand
    {
        public CommandFlags Flags
        {
            get { return CommandFlags.OutOfGame; }
        }

        public string Name
        {
            get { return "shutdown"; }
        }

        public string Help
        {
            get { return "TODO"; }
        }

        public bool Execute(IClient actor, string raw, params CommandParameter[] parameters)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Executing [" + Name + "]");

            return true;
        }
    }
}
