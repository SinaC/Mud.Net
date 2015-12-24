using Mud.Logger;

namespace Mud.Server.Commands
{
    public class Test : ICommand
    {
        public CommandFlags Flags
        {
            get { return CommandFlags.InGame | CommandFlags.OutOfGame; }
        }

        public string Name
        {
            get { return "test"; }
        }

        public string Help
        {
            get { return "Command intended to help debugging parameters parsing"; }
        }

        public bool Execute(IClient actor, string raw, params CommandParameter[] parameters)
        {
            Log.Default.WriteLine(LogLevels.Debug, "TEST COMMAND");
            Log.Default.WriteLine(LogLevels.Debug, "ACTOR: "+actor.Id);
            Log.Default.WriteLine(LogLevels.Debug, "RAW: "+raw);
            Log.Default.WriteLine(LogLevels.Debug, "#PARAMETERS: " + parameters.Length);
            for(int i = 0; i < parameters.Length; i++)
                Log.Default.WriteLine(LogLevels.Debug, "PARAMETER[{0}]: Count: [{1}] Value: [{2}]", i, parameters[i].Count, parameters[i].Value);

            return true;
        }
    }
}
