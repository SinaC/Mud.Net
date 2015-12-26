using Mud.Logger;
using Mud.Server.Commands;

namespace Mud.Server.Old.Commands.OutOfGame
{
    public class TestOutOfGame : IOutOfGameCommand
    {
        public string Name
        {
            get { return "testoog"; }
        }

        public string Help
        {
            get { return "Command intended to help debugging parameters parsing"; }
        }

        public bool Execute(IPlayer player, string rawParameters, params CommandParameter[] parameters)
        {
            Log.Default.WriteLine(LogLevels.Info, "TEST COMMAND");
            Log.Default.WriteLine(LogLevels.Info, "PLAYER: {0}", player.Id);
            Log.Default.WriteLine(LogLevels.Info, "RAW: " + rawParameters);
            Log.Default.WriteLine(LogLevels.Info, "#PARAMETERS: " + parameters.Length);
            for(int i = 0; i < parameters.Length; i++)
                Log.Default.WriteLine(LogLevels.Info, "PARAMETER[{0}]: Count: [{1}] Value: [{2}]", i, parameters[i].Count, parameters[i].Value);

            return true;
        }
    }
}
