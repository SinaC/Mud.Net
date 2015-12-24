using Mud.Logger;

namespace Mud.Server.Commands
{
    // Syntax:
    //  look: look at environment
    //  look <obj>: look at object in character's inventory or environment's inventory
    public class Look : ICommand
    {
        public CommandFlags Flags
        {
            get { return CommandFlags.InGame; }
        }

        public string Name
        {
            get { return "look"; }
        }

        public string Help
        {
            get { return "TODO"; }
        }

        public bool Execute(IClient actor, string raw, params CommandParameter[] parameters)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Executing ["+Name+"]");

            return true;
        }
    }
}
