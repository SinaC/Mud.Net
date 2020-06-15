using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Container;
using Mud.Server.Command;
using Mud.Server.Input;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.GameAction;
using System.Linq;

namespace Mud.Server.Tests.Abilities
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

        protected (string rawParameters, CommandParameter[] parameters) BuildParameters(string parameters)
        {
            var commandParameters = CommandHelpers.SplitParameters(parameters).Select(CommandHelpers.ParseParameter).ToArray();
            return (parameters, commandParameters);
        }

        protected IActionInput BuildActionInput<T>(IActor actor, string parameters)
        {
            return new ActionInput(CommandInfo.Create(typeof(T)), actor, parameters);
        }

        protected IAbilityLearned BuildAbilityLearned(string name)
        {
            var mock = new Mock<IAbilityLearned>();
            mock.SetupGet(x => x.Name).Returns(name);
            mock.SetupGet(x => x.ResourceKind).Returns(Domain.ResourceKinds.Mana);
            mock.SetupGet(x => x.CostAmount).Returns(50);
            mock.SetupGet(x => x.CostAmountOperator).Returns(Domain.CostAmountOperators.Fixed);
            return mock.Object;
        }
    }
}
