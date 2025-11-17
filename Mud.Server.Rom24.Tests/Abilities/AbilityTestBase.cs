using Moq;
using Mud.Server.Flags.Interfaces;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Rom24.Flags;
using Mud.Server.Rom24.Spells;
using System.Reflection;

namespace Mud.Server.Rom24.Tests.Abilities
{
    [TestClass]
    public abstract class AbilityTestBase
    {
        protected IServiceProvider _serviceProvider = default!;

        [TestInitialize]
        public void TestInitialize()
        {
            var serviceProviderMock = new Mock<IServiceProvider>();
            
            serviceProviderMock.Setup(x => x.GetService(typeof(ICharacterFlagValues))) // don't mock IServiceProvider.GetRequiredService because it's an extension method
                .Returns(new CharacterFlagValues());
            serviceProviderMock.Setup(x => x.GetService(typeof(IRoomFlagValues)))
                .Returns(new RoomFlagValues());
            serviceProviderMock.Setup(x => x.GetService(typeof(IOffensiveFlagValues)))
                .Returns(new OffensiveFlagValues());
            serviceProviderMock.Setup(x => x.GetService(typeof(IItemFlagValues)))
                .Returns(new ItemFlagValues());

            RegisterAdditionalDependencies(serviceProviderMock);

            _serviceProvider = serviceProviderMock.Object;
        }

        protected virtual void RegisterAdditionalDependencies(Mock<IServiceProvider> serviceProviderMock)
        {
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

        protected class AssemblyHelper : IAssemblyHelper
        {
            public IEnumerable<Assembly> AllReferencedAssemblies => new[] { typeof(Server.Server).Assembly, typeof(AcidBlast).Assembly };
        }
    }
}
