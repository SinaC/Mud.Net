using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Mud.Common;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.GameAction;
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
        protected IFlagFactory<ICharacterFlags, ICharacterFlagValues> _characterFlagFactory = default!;
        protected IFlagFactory<IOffensiveFlags, IOffensiveFlagValues> _offensiveFlagFactory = default!;
        protected IFlagFactory<IItemFlags, IItemFlagValues> _itemFlagFactory = default!;

        [TestInitialize]
        public void TestInitialize()
        {
            var serviceProviderMock = new Mock<IServiceProvider>();
            _serviceProvider = serviceProviderMock.Object;

            serviceProviderMock.Setup(x => x.GetService(typeof(ICharacterFlagValues))) // don't mock IServiceProvider.GetRequiredService because it's an extension method
                .Returns(() => new CharacterFlagValues(new Mock<ILogger<CharacterFlagValues>>().Object));
            serviceProviderMock.Setup(x => x.GetService(typeof(ICharacterFlags)))
                .Returns(() => new CharacterFlags(_serviceProvider.GetRequiredService<ICharacterFlagValues>()));

            serviceProviderMock.Setup(x => x.GetService(typeof(IRoomFlagValues)))
                .Returns(() => new RoomFlagValues(new Mock<ILogger<RoomFlagValues>>().Object));
            serviceProviderMock.Setup(x => x.GetService(typeof(IRoomFlags)))
                .Returns(() => new RoomFlags(_serviceProvider.GetRequiredService<IRoomFlagValues>()));

            serviceProviderMock.Setup(x => x.GetService(typeof(IItemFlagValues)))
                .Returns(() => new ItemFlagValues(new Mock<ILogger<ItemFlagValues>>().Object));
            serviceProviderMock.Setup(x => x.GetService(typeof(IItemFlags)))
                .Returns(() => new ItemFlags(_serviceProvider.GetRequiredService<IItemFlagValues>()));

            serviceProviderMock.Setup(x => x.GetService(typeof(IWeaponFlagValues)))
                .Returns(() => new WeaponFlagValues(new Mock<ILogger<WeaponFlagValues>>().Object));
            serviceProviderMock.Setup(x => x.GetService(typeof(IWeaponFlags)))
                .Returns(() => new WeaponFlags(_serviceProvider.GetRequiredService<IWeaponFlagValues>()));

            serviceProviderMock.Setup(x => x.GetService(typeof(IIRVFlagValues)))
                .Returns(() => new IRVFlagValues(new Mock<ILogger<IRVFlagValues>>().Object));
            serviceProviderMock.Setup(x => x.GetService(typeof(IIRVFlags)))
                .Returns(() => new IRVFlags(_serviceProvider.GetRequiredService<IIRVFlagValues>()));

            serviceProviderMock.Setup(x => x.GetService(typeof(IOffensiveFlagValues)))
                .Returns(() => new OffensiveFlagValues(new Mock<ILogger<OffensiveFlagValues>>().Object));
            serviceProviderMock.Setup(x => x.GetService(typeof(IOffensiveFlags)))
                .Returns(() => new OffensiveFlags(_serviceProvider.GetRequiredService<IOffensiveFlagValues>()));

            _characterFlagFactory = new CharacterFlagsFactory(_serviceProvider);
            _offensiveFlagFactory = new OffensiveFlagsFactory(_serviceProvider);
            _itemFlagFactory = new ItemFlagsFactory(_serviceProvider);

            RegisterAdditionalDependencies(serviceProviderMock);
        }

        protected virtual void RegisterAdditionalDependencies(Mock<IServiceProvider> serviceProviderMock)
        {
        }

        protected static ICommandParameter[] BuildParameters(string parameters)
        {
            var parser = new CommandParser(new Mock<ILogger<CommandParser>>().Object);
            return parser.SplitParameters(parameters).Select(parser.ParseParameter).ToArray();
        }

        protected static IActionInput BuildActionInput<TGameAction>(IActor actor, string commandLine)
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
                    gameActionInfo = new AdminGameActionInfo(type, adminCommandAttribute, syntaxAttribute, aliasAttributes, null);
                    break;
                case PlayerCommandAttribute playerCommandAttribute:
                    gameActionInfo = new PlayerGameActionInfo(type, playerCommandAttribute, syntaxAttribute, aliasAttributes, null);
                    break;
                case PlayableCharacterCommandAttribute playableCharacterCommandAttribute:
                    gameActionInfo = new PlayableCharacterGameActionInfo(type, playableCharacterCommandAttribute, syntaxAttribute, aliasAttributes, null);
                    break;
                case CharacterCommandAttribute characterCommandAttribute:
                    gameActionInfo = new CharacterGameActionInfo(type, characterCommandAttribute, syntaxAttribute, aliasAttributes, null);
                    break;
                default:
                    gameActionInfo = new GameActionInfo(type, commandAttribute, syntaxAttribute, aliasAttributes, null);
                    break;
            }

            new CommandParser(new Mock<ILogger<CommandParser>>().Object).ExtractCommandAndParameters(commandLine, out var command, out var parameters);
            return new ActionInput(gameActionInfo, actor, commandLine, command, parameters);
        }

        protected static IAbilityLearned BuildAbilityLearned(string name)
        {
            var mock = new Mock<IAbilityLearned>();
            mock.SetupGet(x => x.Name).Returns(name);
            mock.SetupGet(x => x.ResourceKind).Returns(Domain.ResourceKinds.Mana);
            mock.SetupGet(x => x.CostAmount).Returns(50);
            mock.SetupGet(x => x.CostAmountOperator).Returns(Domain.CostAmountOperators.Fixed);
            mock.Setup(x => x.HasCost()).Returns(true);
            return mock.Object;
        }

        public class AssemblyHelper : IAssemblyHelper
        {
            public IEnumerable<Assembly> AllReferencedAssemblies => new[] { typeof(Server.Server).Assembly, typeof(AcidBlast).Assembly };
        }
    }
}
