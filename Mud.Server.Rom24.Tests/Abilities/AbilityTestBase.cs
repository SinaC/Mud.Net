using Microsoft.Extensions.Logging;
using Moq;
using Mud.Common;
using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Rom24.Spells;
using System.Reflection;

namespace Mud.Server.Rom24.Tests.Abilities;

[TestClass]
public abstract class AbilityTestBase
{
    protected IServiceProvider _serviceProvider = default!;

    [TestInitialize]
    public void TestInitialize()
    {
        var serviceProviderMock = new Mock<IServiceProvider>();
        _serviceProvider = serviceProviderMock.Object;

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
        CommandAttribute commandAttribute = type.GetCustomAttribute<CommandAttribute>()!;
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
        mock.SetupGet(x => x.ResourceKind).Returns(ResourceKinds.Mana);
        mock.SetupGet(x => x.CostAmount).Returns(50);
        mock.SetupGet(x => x.CostAmountOperator).Returns(CostAmountOperators.Fixed);
        mock.Setup(x => x.HasCost()).Returns(true);
        return mock.Object;
    }

    public class AssemblyHelper : IAssemblyHelper
    {
        public IEnumerable<Assembly> AllReferencedAssemblies => [typeof(Server.Server).Assembly, typeof(AcidBlast).Assembly];
    }
}
