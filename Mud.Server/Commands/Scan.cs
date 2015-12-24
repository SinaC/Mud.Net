using Mud.Logger;

namespace Mud.Server.Commands
{
    class Scan : ICommand
    {
        public CommandFlags Flags
        {
            get { return CommandFlags.InGame; }
        }

        public string Name
        {
            get { return "scan"; }
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
