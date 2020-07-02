using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Container;
using Mud.Server.Flags.Interfaces;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.GameAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
            DependencyContainer.Current.RegisterInstance<ICharacterFlagValues>(new Rom24CharacterFlagValues());
            DependencyContainer.Current.RegisterInstance<IRoomFlagValues>(new Rom24RoomFlagValues());
            DependencyContainer.Current.RegisterInstance<IOffensiveFlagValues>(new Rom24OffensiveFlagValues());
        }

        [TestCleanup]
        public void TestCleanup()
        {
            DependencyContainer.SetManualContainer(_originalContainer);
        }

        protected ICommandParameter[] BuildParameters(string parameters)
        {
            return CommandHelpers.SplitParameters(parameters).Select(CommandHelpers.ParseParameter).ToArray();
        }

        protected IActionInput BuildActionInput<TGameAction>(IActor actor, string commandLine)
            where TGameAction:IGameAction
        {
            Type type = typeof(TGameAction);
            CommandAttribute commandAttribute = type.GetCustomAttribute<CommandAttribute>();
            SyntaxAttribute syntaxAttribute = type.GetCustomAttribute<SyntaxAttribute>() ?? GameActionInfo.DefaultSyntaxCommandAttribute;
            IEnumerable<AliasAttribute> aliasAttributes = type.GetCustomAttributes<AliasAttribute>();

            IGameActionInfo gameActionInfo;
            switch (commandAttribute)
            {
                case AdminCommandAttribute adminCommandAttribute:
                    gameActionInfo = new AdminGameActionInfo(type, adminCommandAttribute, syntaxAttribute, aliasAttributes);
                    break;
                case PlayerCommandAttribute playerCommandAttribute:
                    gameActionInfo = new PlayerGameActionInfo(type, playerCommandAttribute, syntaxAttribute, aliasAttributes);
                    break;
                case PlayableCharacterCommandAttribute playableCharacterCommandAttribute:
                    gameActionInfo = new PlayableCharacterGameActionInfo(type, playableCharacterCommandAttribute, syntaxAttribute, aliasAttributes);
                    break;
                case CharacterCommandAttribute characterCommandAttribute:
                    gameActionInfo = new CharacterGameActionInfo(type, characterCommandAttribute, syntaxAttribute, aliasAttributes);
                    break;
                default:
                    gameActionInfo = new GameActionInfo(type, commandAttribute, syntaxAttribute, aliasAttributes);
                    break;
            }

            CommandHelpers.ExtractCommandAndParameters(commandLine, out var command, out var parameters);
            return new ActionInput(gameActionInfo, actor, commandLine, command, parameters);
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
