using System.Linq;
using Mud.Server.Input;

namespace Mud.POC.Tests.Followers
{
    public abstract class FollowerTestBase
    {
        //
        protected (string rawParameters, CommandParameter[] parameters) BuildParameters(string parameters)
        {
            var commandParameters = CommandHelpers.SplitParameters(parameters).Select(CommandHelpers.ParseParameter).ToArray();
            return (parameters, commandParameters);
        }

        protected (string rawParameters, CommandParameter[] parameters) BuildParametersSkipFirst(string parameters)
        {
            return CommandHelpers.SkipParameters(CommandHelpers.SplitParameters(parameters).Select(CommandHelpers.ParseParameter), 1);
        }
    }
}
