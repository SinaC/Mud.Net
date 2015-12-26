using Mud.Logger;
using Mud.Server.Commands;

namespace Mud.Server.Old.Commands.InGame
{
    public class TestInGame : IInGameCommand
    {
        public string Name
        {
            get { return "testig"; }
        }

        public string Help
        {
            get { return "Command intended to help debugging parameters parsing"; }
        }

        public bool Execute(IEntity entity, string rawParameters, params CommandParameter[] parameters)
        {
            Log.Default.WriteLine(LogLevels.Info, "TEST COMMAND");
            Log.Default.WriteLine(LogLevels.Info, "ENTITY: {0}", entity.Id);
            Log.Default.WriteLine(LogLevels.Info, "RAW: " + rawParameters);
            Log.Default.WriteLine(LogLevels.Info, "#PARAMETERS: " + parameters.Length);
            for(int i = 0; i < parameters.Length; i++)
                Log.Default.WriteLine(LogLevels.Info, "PARAMETER[{0}]: Count: [{1}] Value: [{2}]", i, parameters[i].Count, parameters[i].Value);

            return true;
        }
    }
}
