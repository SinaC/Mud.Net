using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.POC.GroupsPetsFollowers;
using Mud.Server.Input;

namespace Mud.POC.Tests.Followers
{
    [TestClass]
    public abstract class FollowerTestBase
    {
        [TestInitialize]
        public void TestInitialize()
        {
            World.Instance.Clear();
        }

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
