using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.Container;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using System.Linq;

namespace Mud.POC.Tests.Abilities2
{
    [TestClass]
    public abstract class TestBase
    {
        private SimpleInjector.Container _originalContainer;

        [TestInitialize]
        public void TestInitialize()
        {
            _originalContainer = DependencyContainer.Current;
            DependencyContainer.SetManualContainer(new SimpleInjector.Container());
        }

        [TestCleanup]
        public void TestCleanup()
        {
            DependencyContainer.SetManualContainer(_originalContainer);
        }

        protected (string rawParameters, ICommandParameter[] parameters) BuildParameters(string parameters)
        {
            var commandParameters = CommandHelpers.SplitParameters(parameters).Select(CommandHelpers.ParseParameter).ToArray();
            return (parameters, commandParameters);
        }

        protected POC.Abilities2.ActionInput BuildActionInput(IActor actor, string parameters)
        {
            return new POC.Abilities2.ActionInput(actor, parameters);
        }
    }
}
